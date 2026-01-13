using Caliburn.Micro;

namespace KeeperWpf;

public class SettingsViewModel : Screen
{
    public LargeExpenseThresholdViewModel LargeExpenseThresholdViewModel { get; set; }

    public SettingsViewModel(LargeExpenseThresholdViewModel largeExpenseThresholdViewModel)
    {
        LargeExpenseThresholdViewModel = largeExpenseThresholdViewModel;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Настрйки";
    }
}
