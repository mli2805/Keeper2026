namespace KeeperDomain;

public class CustomReminder
{
    public int Id { get; set; }
    public DateTime TriggerDate { get; set; } // когда сработает напоминание (подсветится колокольчик)
    public Duration Every { get; set; } = new Duration(); // сколько добавить к дате сработки, если не одноразовое напоминание

    public string Memo { get; set; } = string.Empty;
}
