using KeeperDomain;

namespace KeeperModels;

public class CustomReminderModel
{
    public int Id { get; set; }
    public bool Enabled { get; set; } = true; // если выключено, то колокольчик не подсветится, но напоминание останется в списке

    public DateTime TriggerDate { get; set; } = DateTime.Now.Date; // когда сработает напоминание (подсветится колокольчик)
    public Duration Every { get; set; } = new Duration(1, Durations.Years); // сколько добавить к дате сработки, если не одноразовое напоминание

    public string Memo { get; set; } = string.Empty;


    // -------------------------

    public string EveryStr => Every.ToRussianString(false);
    public bool IsOnce { get; set; }
    public bool IsRepeated { get; set; }
    
    public bool IsTimeToTrigger()
    {
        return Enabled && TriggerDate.Date <= DateTime.Now.Date;
    }

}
