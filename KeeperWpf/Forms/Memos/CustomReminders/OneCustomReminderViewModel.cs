using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

public class OneCustomReminderViewModel(CustomRemindersRepository customRemindersRepository) : Screen
{
    private bool _isAddMode;

    public CustomReminderModel CustomReminderInWork { get; private set; } = null!;
    public List<Durations> Durations { get; set; } = Enum.GetValues(typeof(Durations)).OfType<Durations>().ToList();

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _isAddMode ? "Добавить напоминание" : "Редактировать напоминание";
    }

    public void Initialize(CustomReminderModel? customReminder)
    {
        _isAddMode = customReminder == null;
        CustomReminderInWork = customReminder ?? new CustomReminderModel();
        CustomReminderInWork.IsOnce = CustomReminderInWork.Every.IsPerpetual;
        CustomReminderInWork.IsRepeated = !CustomReminderInWork.Every.IsPerpetual;

    }

    public async Task Save()
    {
        if (_isAddMode)
        {
            CustomReminderInWork = await customRemindersRepository.Add(CustomReminderInWork);
        }
        else
        {
            CustomReminderInWork = await customRemindersRepository.Update(CustomReminderInWork);
        }

        await TryCloseAsync();
    }

    public async Task Cancel()
    {
        await TryCloseAsync();
    }
}