using Caliburn.Micro;
using KeeperInfrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class LargeExpenseThresholdViewModel(KeeperDataModel keeperDataModel,
    LargeExpenseThresholdsRepository largeExpenseThresholdsRepository) : Screen
{
    public KeeperDataModel KeeperDataModel { get; } = keeperDataModel;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Настройка порог крупных покупок";
    }
 
    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        await largeExpenseThresholdsRepository.SaveAll(KeeperDataModel.LargeExpenseThresholds);
        return true;
    }   
}
