using KeeperDomain;

namespace KeeperModels;

public class CustomReminderModel
{
    public int Id { get; set; }
    public bool Enabled { get; set; } // если выключено, то колокольчик не подсветится, но напоминание останется в списке

    public DateTime TriggerDate { get; set; } // когда сработает напоминание (подсветится колокольчик)
    public Duration? Every { get; set; } // сколько добавить к дате сработки, если не одноразовое напоминание

    public string Memo { get; set; } = string.Empty;

    // -------------------------

    public bool IsTimeToTrigger()
    {
        return Enabled && TriggerDate <= DateTime.Now;
    }

}
