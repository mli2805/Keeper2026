using Caliburn.Micro;
using KeeperInfrastructure;
using KeeperModels;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class CustomRemindersViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager,
    OneCustomReminderViewModel oneCustomReminderViewModel, CustomRemindersRepository customRemindersRepository) : Screen
{
    public BindableCollection<CustomReminderModel> CustomReminders { get; set; } = null!;

    public CustomReminderModel? SelectedCustomReminder { get; set; }

    public void Initialize()
    {
        CustomReminders = [.. keeperDataModel.CustomReminderModels.OrderBy(cr => cr.TriggerDate)];
        if (CustomReminders.Count > 0)
        {
            SelectedCustomReminder = CustomReminders[0];
        }
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Напоминания";
    }

    public async Task AddReminder()
    {
        oneCustomReminderViewModel.Initialize(null);
        await windowManager.ShowDialogAsync(oneCustomReminderViewModel);
        var newCustomReminderModel = oneCustomReminderViewModel.CustomReminderInWork;
        if (newCustomReminderModel != null)
        {
            CustomReminders.Add(newCustomReminderModel);
            keeperDataModel.CustomReminderModels.Add(newCustomReminderModel);
            SelectedCustomReminder = newCustomReminderModel;
        }
    }

    public async Task EditReminder()
    {
        oneCustomReminderViewModel.Initialize(SelectedCustomReminder);
        await windowManager.ShowDialogAsync(oneCustomReminderViewModel);
        var updatedCustomReminderModel = oneCustomReminderViewModel.CustomReminderInWork;
        if (updatedCustomReminderModel != null)
        {
            var index = CustomReminders.IndexOf(SelectedCustomReminder!);
            if (index >= 0)
            {
                CustomReminders[index] = updatedCustomReminderModel;
                SelectedCustomReminder = updatedCustomReminderModel;
            }

            var idx = keeperDataModel.CustomReminderModels.FindIndex(cr => cr.Id == updatedCustomReminderModel.Id);
            if (idx >= 0)
            {
                keeperDataModel.CustomReminderModels[idx] = updatedCustomReminderModel;
            }
        }
    }

    public async Task DeleteReminder()
    {
        var vm = new MyMessageBoxViewModel(MessageType.Confirmation, "Вы уверены, что хотите удалить это напоминание?");
        var result = await windowManager.ShowDialogAsync(vm);

        if (result.HasValue && result.Value)
        {
            await customRemindersRepository.Delete(SelectedCustomReminder!.Id);
            CustomReminders.Remove(SelectedCustomReminder!);
            keeperDataModel.CustomReminderModels.Remove(SelectedCustomReminder!);
        }
    }

    public async Task CloseView()
    {
        await TryCloseAsync();
    }
}
