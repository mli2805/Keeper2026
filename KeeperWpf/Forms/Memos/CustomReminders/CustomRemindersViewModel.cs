using Caliburn.Micro;
using KeeperModels;
using System.Threading.Tasks;

namespace KeeperWpf;

public class CustomRemindersViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager,
    OneCustomReminderViewModel oneCustomReminderViewModel) : Screen
{
    public BindableCollection<CustomReminderModel> CustomReminders { get; } = new();

    public void Initialize()
    {
        CustomReminders.AddRange(keeperDataModel.CustomReminderModels);
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Напоминания";
    }

    public async Task AddReminder()
    {
        await windowManager.ShowDialogAsync(oneCustomReminderViewModel);
    }
}
