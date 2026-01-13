namespace KeeperWpf;

public class LargeExpenseThresholdViewModel
{
    public KeeperDataModel KeeperDataModel { get; set; }

    public LargeExpenseThresholdViewModel(KeeperDataModel keeperDataModel)
    {
        KeeperDataModel = keeperDataModel;
    }
}
