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
        var confirmation = await windowManager.ShowDialogAsync(oneCustomReminderViewModel);
        if (confirmation == false) return;

        var newCustomReminderModel = oneCustomReminderViewModel.CustomReminderInWork;
        keeperDataModel.CustomReminderModels.Add(newCustomReminderModel);
        CustomReminders.Clear();
        CustomReminders.AddRange(keeperDataModel.CustomReminderModels.OrderBy(cr => cr.TriggerDate));

        SelectedCustomReminder = CustomReminders.FirstOrDefault(cr => cr.Id == newCustomReminderModel.Id);
    }

    public async Task EditReminder()
    {
        var clone = SelectedCustomReminder!.ShallowCopy();

        oneCustomReminderViewModel.Initialize(clone);
        var confirmation = await windowManager.ShowDialogAsync(oneCustomReminderViewModel);
        if (confirmation == false) return;

        var updatedCustomReminderModel = oneCustomReminderViewModel.CustomReminderInWork;
        var idx = keeperDataModel.CustomReminderModels.FindIndex(cr => cr.Id == updatedCustomReminderModel.Id);
        if (idx >= 0)
        {
            keeperDataModel.CustomReminderModels[idx] = updatedCustomReminderModel;
        }
        CustomReminders.Clear();
        CustomReminders.AddRange(keeperDataModel.CustomReminderModels.OrderBy(cr => cr.TriggerDate));

        SelectedCustomReminder = CustomReminders.FirstOrDefault(cr => cr.Id == updatedCustomReminderModel.Id);
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
