using Caliburn.Micro;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class OneMonthReadingsViewModel : Screen
{
    public UtilityMeters UtilityMetersInWork { get; private set; } = null!;

    public DateTime? ReadingsDate
    {
        get => UtilityMetersInWork.ReadingsDate.ToDateTime(TimeOnly.MinValue);
        set
        {
            if (value.HasValue)
            {
                UtilityMetersInWork.ReadingsDate = DateOnly.FromDateTime(value.Value);
            }
        }
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"Показания за {UtilityMetersInWork.ReadingsDate : MMMM yyyy}";
    }

    public void Initialize(UtilityMeters utilityMeters)
    {
        UtilityMetersInWork = utilityMeters;
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
