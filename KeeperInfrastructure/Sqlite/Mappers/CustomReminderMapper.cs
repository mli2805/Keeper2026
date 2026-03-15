using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class CustomReminderMapper
{
    public static CustomReminderEf ToEf(this CustomReminder domain)
    {
        return new CustomReminderEf
        {
            Id = domain.Id,
            Enabled = domain.Enabled,
            TriggerDate = domain.TriggerDate,
            Every = domain.Every?.ToString(),
            Memo = domain.Memo
        };
    }

    public static CustomReminder FromEf(this CustomReminderEf ef)
    {
        return new CustomReminder
        {
            Id = ef.Id,
            Enabled = ef.Enabled,
            TriggerDate = ef.TriggerDate,
            Every = Duration.FromNullableString(ef.Every),
            Memo = ef.Memo
        };
    }

    public static CustomReminderModel ToModel(this CustomReminderEf ef)
    {
        return new CustomReminderModel
        {
            Id = ef.Id,
            Enabled = ef.Enabled,
            TriggerDate = ef.TriggerDate,
            Every = Duration.FromNullableString(ef.Every),
            Memo = ef.Memo
        };
    }

    public static CustomReminder FromModel(this CustomReminderModel model)
    {
        return new CustomReminder
        {
            Id = model.Id,
            Enabled = model.Enabled,
            TriggerDate = model.TriggerDate,
            Every = model.Every,
            Memo = model.Memo
        };
    }
}
