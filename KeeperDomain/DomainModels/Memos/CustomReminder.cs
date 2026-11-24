namespace KeeperDomain;

public class CustomReminder : IDumpable, IParsable<CustomReminder>
{
    public int Id { get; set; }
    public DateTime TriggerDate { get; set; } // когда сработает напоминание (подсветится колокольчик)
    public Duration Every { get; set; } = new Duration(); // сколько добавить к дате сработки, если не одноразовое напоминание

    public string Memo { get; set; } = string.Empty;

    public string Dump()
    {
        return Id + " ; " + TriggerDate.ToString("dd/MM/yyyy HH:mm") + " ; " + 
            Every.Dump(false) + " ; " + Memo.Replace("\r\n", "|");
    }

    public CustomReminder FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        TriggerDate = DateTime.ParseExact(substrings[1].Trim(), "dd/MM/yyyy HH:mm", null);
        Every = new Duration().FromString(substrings[2].Trim());
        Memo = substrings[3].Trim().Replace("|", "\r\n");
        return this;
    }
}
