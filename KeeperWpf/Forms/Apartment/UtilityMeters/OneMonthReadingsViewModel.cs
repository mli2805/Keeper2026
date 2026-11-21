using Caliburn.Micro;
using KeeperInfrastructure;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class OneMonthReadingsViewModel(KeeperDataModel dataModel, 
    HouseholdRepository householdRepository) : Screen
{
    public UtilityMetersModel UtilityMetersInWork { get; private set; } = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"Показания за {UtilityMetersInWork.ReadingsDate : MMMM yyyy}";
    }

    private bool _isReadingsJustCreated;
    public void Initialize()
    {
        UtilityMetersInWork = dataModel.GetLastOrNew();
        _isReadingsJustCreated = UtilityMetersInWork.Id == 0;

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
        if (_isReadingsJustCreated)
        {
            var savedReadings = await householdRepository.AddUtilityMeters(UtilityMetersInWork);
            dataModel.UtilityMeters.Add(savedReadings);
        }
        else
        {
            await householdRepository.UpdateUtilityMeters(UtilityMetersInWork);
        }

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
