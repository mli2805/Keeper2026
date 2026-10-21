using Caliburn.Micro;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class OneMonthReadingsViewModel : Screen
{
    public UtilityMetersModel UtilityMetersInWork { get; private set; } = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"Показания за {UtilityMetersInWork.ReadingsDate : MMMM yyyy}";
    }

    public void Initialize(UtilityMetersModel utilityMeters)
    {
        UtilityMetersInWork = utilityMeters;

        UtilityMetersInWork.ColdWater.PropertyChanged += UtilityMeter_PropertyChanged;
        UtilityMetersInWork.HotWater.PropertyChanged += UtilityMeter_PropertyChanged;
        UtilityMetersInWork.Electricity.PropertyChanged += UtilityMeter_PropertyChanged;
        UtilityMetersInWork.Gas.PropertyChanged += UtilityMeter_PropertyChanged;
    }

    private void UtilityMeter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not UtilityMeterModel utilityMeter)
            return;

        if (e.PropertyName == nameof(UtilityMeterModel.Volume))
            utilityMeter.To = utilityMeter.From + utilityMeter.Volume;
        else if (e.PropertyName == nameof(UtilityMeterModel.To))
            utilityMeter.Volume = utilityMeter.To - utilityMeter.From;
    }

    public async Task Save()
    {
        await TryCloseAsync(true);
    }

    public async Task Cancel()
    {
        await TryCloseAsync(false);
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        return await base.CanCloseAsync(cancellationToken);
    }
}
