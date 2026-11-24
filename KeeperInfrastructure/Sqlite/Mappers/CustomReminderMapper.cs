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
            TriggerDate = model.TriggerDate,
            Every = model.Every.Dump(false),
            Memo = model.Memo
        };
        return entity;
    }

    public static CustomReminderEf ToEf(this CustomReminder item)
    {
        CustomReminderEf entity = new()
        {
            Id = item.Id,
            TriggerDate = item.TriggerDate,
            Every = item.Every.Dump(false),
            Memo = item.Memo
        };
        return entity;
    }

  

    public static CustomReminderModel ToModel(this CustomReminderEf ef)
    {
        return new CustomReminderModel
        {
            Id = ef.Id,
            TriggerDate = ef.TriggerDate,
            Every = new Duration().FromString(ef.Every),
            Memo = ef.Memo
        };
    }
}
