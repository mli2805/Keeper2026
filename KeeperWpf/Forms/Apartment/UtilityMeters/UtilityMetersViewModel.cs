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

    //деноминация
    public async Task Fix()
    {
        foreach (var row in dataModel.UtilityMeters)
        {
            if (row.ReadingsDate < new DateTime(2016, 7, 1))
                continue;

            var elTariff = row.Electricity.Tariff.ToString("0");
            while (elTariff.Length < 4)
                elTariff += "0";
            row.Electricity.Tariff = double.Parse(elTariff) / 10_000;

            var gasTariff = row.Gas.Tariff.ToString("0");
            while (gasTariff.Length < 4)
                gasTariff += "0";
            row.Gas.Tariff = double.Parse(gasTariff) / 10_000;
        }
        NotifyOfPropertyChange(nameof(Rows));
        await Task.CompletedTask;
    }

    public async Task AddCurrentMonth()
    {
        oneMonthReadingsViewModel.Initialize();
        if (await windowManager.ShowDialogAsync(oneMonthReadingsViewModel) != true) return;

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
