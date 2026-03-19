using System.Linq;

namespace KeeperWpf;

public static class ReminderExt
{
    public static bool HasRemindersTriggered(this KeeperDataModel keeperDataModel)
    {
        return keeperDataModel.CustomReminderModels.Any(r => r.IsTimeToTrigger());
    }
}
