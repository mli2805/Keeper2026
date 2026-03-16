using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class CustomReminderMapper
{
    public static CustomReminderEf ToEf(this CustomReminderModel model)
    {
        model.Every.IsPerpetual = model.IsOnce;

        CustomReminderEf entity = new()
        {
            Id = model.Id,
            Enabled = model.Enabled,
            TriggerDate = model.TriggerDate,
            Every = model.Every.Dump(false),
            Memo = model.Memo
        };
        return entity;
    }

    public static CustomReminder FromEf(this CustomReminderEf ef)
    {
        return new CustomReminder
        {
            Id = ef.Id,
            Enabled = ef.Enabled,
            TriggerDate = ef.TriggerDate,
            Every = new Duration().FromString(ef.Every),
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
            Every = new Duration().FromString(ef.Every),
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
