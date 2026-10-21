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

        NotifyOfPropertyChange(nameof(Rows));
    }
}
