namespace KeeperDomain;

public class CustomReminder : IDumpable, IParsable<CustomReminder>
{
    public int Id { get; set; }
    public bool Enabled { get; set; } // если выключено, то колокольчик не подсветится, но напоминание останется в списке

    public DateTime TriggerDate { get; set; } // когда сработает напоминание (подсветится колокольчик)
    public Duration? Every { get; set; } // сколько добавить к дате сработки, если не одноразовое напоминание

    public string Memo { get; set; } = string.Empty;

    public string Dump()
    {
        return Id + " ; " + Enabled + " ; " + TriggerDate.ToString("dd/MM/yyyy HH:mm") + " ; " + Every?.ToString() + " ; " + Memo;
    }

    public CustomReminder FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        Enabled = bool.Parse(substrings[1]);
        TriggerDate = DateTime.ParseExact(substrings[2], "dd/MM/yyyy HH:mm", null);
        Every = Duration.FromNullableString(substrings[3]);
        Memo = substrings[4].Trim();
        return this;
    }
}
