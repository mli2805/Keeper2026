using Caliburn.Micro;
using KeeperDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class UtilityMetersViewModel(KeeperDataModel dataModel, IWindowManager windowManager,
    OneMonthReadingsViewModel oneMonthReadingsViewModel) : Screen
{
    public IReadOnlyList<UtilityMeters> Rows => dataModel.UtilityMeters;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Показания счетчиков";
    }

    public async Task AddCurrentMonth()
    {
        var monthReadings = new UtilityMeters
        {
            ReadingsDate = DateOnly.FromDateTime(DateTime.Now),
        };
        UtilityMeters previousMonth = dataModel.UtilityMeters.Last();
        monthReadings.ColdWater.From = previousMonth.ColdWater.To;
        monthReadings.HotWater.From = previousMonth.HotWater.To;
        monthReadings.Electricity.From = previousMonth.Electricity.To;
        monthReadings.Gas.From = previousMonth.Gas.To;

        oneMonthReadingsViewModel.Initialize(monthReadings);
        await windowManager.ShowWindowAsync(oneMonthReadingsViewModel);
    }
}
