using Caliburn.Micro;
using KeeperInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class UtilityMetersViewModel(KeeperDataModel dataModel, IWindowManager windowManager,
    OneMonthReadingsViewModel oneMonthReadingsViewModel, HouseholdRepository householdRepository) : Screen
{
    public IReadOnlyList<UtilityMetersModel> Rows => dataModel.UtilityMeters;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Показания счетчиков";
        ApplyTransactionComments();
    }

    public async Task ImportGasReadings()
    {
        GasReadingsImporter.Import(
            dataModel.Transactions.Values,
            dataModel.UtilityMeters);

        ApplyTransactionComments();
        NotifyOfPropertyChange(nameof(Rows));
        await Task.CompletedTask;
    }

    public async Task CalculateGasReadings()
    {
        GasReadingsImporter.CalculatePreviousReadings(dataModel.UtilityMeters);

        NotifyOfPropertyChange(nameof(Rows));
        await Task.CompletedTask;
    }

    public async Task CalculateElectricityRate()
    {
        var electricityAmounts = dataModel.Transactions.Values
            .Where(transaction => string.Equals(
                transaction.Category?.Name,
                "Электричество",
                StringComparison.OrdinalIgnoreCase))
            .GroupBy(transaction => (transaction.Timestamp.Year, transaction.Timestamp.Month))
            .ToDictionary(group => group.Key, group => group.Sum(transaction => transaction.Amount));
        var gasAmounts = dataModel.Transactions.Values
            .Where(transaction => string.Equals(
                transaction.Category?.Name,
                "Газ",
                StringComparison.OrdinalIgnoreCase))
            .GroupBy(transaction => (transaction.Timestamp.Year, transaction.Timestamp.Month))
            .ToDictionary(group => group.Key, group => group.Sum(transaction => transaction.Amount));

        foreach (var row in dataModel.UtilityMeters)
        {
            var month = (row.ReadingsDate.Year, row.ReadingsDate.Month);
            var decimalPlaces = row.ReadingsDate < new DateTime(2016, 7, 1) ? 2 : 4;
            var hasElectricityAmount = electricityAmounts.TryGetValue(month, out var electricityAmount);
            var hasGasAmount = gasAmounts.TryGetValue(month, out var gasAmount);
            row.ElectricityPaymentAmount = hasElectricityAmount ? electricityAmount : null;
            row.GasPaymentAmount = hasGasAmount ? gasAmount : null;

            if (row.Electricity.Tariff == 0)
            {
                row.Electricity.Tariff = row.Electricity.Volume != 0
                    && hasElectricityAmount
                        ? (double)Math.Round(electricityAmount / (decimal)row.Electricity.Volume, decimalPlaces)
                        : 0;
            }

            if (row.Gas.Tariff == 0)
            {
                row.Gas.Tariff = row.Gas.Volume != 0
                    && hasGasAmount
                        ? (double)Math.Round(gasAmount / (decimal)row.Gas.Volume, decimalPlaces)
                        : 0;
            }
        }

        await Task.CompletedTask;
    }

    public async Task SaveManualCorrections()
    {
        foreach (var row in dataModel.UtilityMeters)
            await householdRepository.UpdateUtilityMeters(row);
    }

    public async Task AddCurrentMonth()
    {
        UtilityMetersModel lastReadings = dataModel.UtilityMeters.Last();
        var now = DateTime.Now;
        var isCurrentMonth = lastReadings.ReadingsDate.Year == now.Year
            && lastReadings.ReadingsDate.Month == now.Month;

        UtilityMetersModel monthReadings;
        if (isCurrentMonth)
        {
            monthReadings = lastReadings;
        }
        else
        {
            monthReadings = new UtilityMetersModel
            {
                ReadingsDate = now,
            };
            monthReadings.ColdWater.From = lastReadings.ColdWater.To;
            monthReadings.HotWater.From = lastReadings.HotWater.To;
            monthReadings.Electricity.From = lastReadings.Electricity.To;
            monthReadings.Gas.From = lastReadings.Gas.To;
        }

        oneMonthReadingsViewModel.Initialize(monthReadings);
        if (await windowManager.ShowDialogAsync(oneMonthReadingsViewModel) != true) return;

        if (isCurrentMonth)
        {
            await householdRepository.UpdateUtilityMeters(oneMonthReadingsViewModel.UtilityMetersInWork);
        }
        else
        {
            var savedReadings = await householdRepository.AddUtilityMeters(oneMonthReadingsViewModel.UtilityMetersInWork);
            dataModel.UtilityMeters.Add(savedReadings);
        }

        ApplyTransactionComments();
        NotifyOfPropertyChange(nameof(Rows));
    }

    private void ApplyTransactionComments()
    {
        var electricityComments = GetTransactionComments("Электричество");
        var gasComments = GetTransactionComments("Газ");

        foreach (var row in dataModel.UtilityMeters)
        {
            var month = (row.ReadingsDate.Year, row.ReadingsDate.Month);
            row.ElectricityComment = electricityComments.TryGetValue(month, out var electricityComment)
                ? electricityComment
                : string.Empty;
            row.GasComment = gasComments.TryGetValue(month, out var gasComment)
                ? gasComment
                : string.Empty;
        }
    }

    private Dictionary<(int Year, int Month), string> GetTransactionComments(string categoryName) =>
        dataModel.Transactions.Values
            .Where(transaction =>
                string.Equals(transaction.Category?.Name, categoryName, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(transaction.Comment))
            .OrderBy(transaction => transaction.Timestamp)
            .GroupBy(transaction => (transaction.Timestamp.Year, transaction.Timestamp.Month))
            .ToDictionary(
                group => group.Key,
                group => string.Join("; ", group.Select(transaction => transaction.Comment.Trim())));
}
