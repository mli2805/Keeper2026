using Caliburn.Micro;
using KeeperInfrastructure;
using KeeperModels;
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
        var monthReadings = new UtilityMetersModel
        {
            ReadingsDate = DateTime.Now,
        };
        UtilityMetersModel previousMonth = dataModel.UtilityMeters.Last();
        monthReadings.ColdWater.From = previousMonth.ColdWater.To;
        monthReadings.HotWater.From = previousMonth.HotWater.To;
        monthReadings.Electricity.From = previousMonth.Electricity.To;
        monthReadings.Gas.From = previousMonth.Gas.To;

        oneMonthReadingsViewModel.Initialize(monthReadings);
        if (await windowManager.ShowDialogAsync(oneMonthReadingsViewModel) != true) return;

        var savedReadings = await householdRepository.AddUtilityMeters(oneMonthReadingsViewModel.UtilityMetersInWork);
        dataModel.UtilityMeters.Add(savedReadings);
        NotifyOfPropertyChange(nameof(Rows));
    }
}
