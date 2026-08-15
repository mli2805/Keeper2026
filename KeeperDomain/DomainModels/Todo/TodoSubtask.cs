using System.ComponentModel.DataAnnotations;

namespace KeeperDomain;

public class TodoSubtask : IDumpable, IParsable<TodoSubtask>
{
    public int Id { get; set; }
    public int TodoTaskId { get; set; }
    public int Ordinal { get; set; }
    [MaxLength(1000)] public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    public string Dump()
    {
        return Id + " ; " + TodoTaskId + " ; " + Ordinal + " ; " + Title + " ; " + IsCompleted;
    }

    public TodoSubtask FromString(string s)
    {
        var substrings = s.Split(';');

        Id = int.Parse(substrings[0].Trim());
        TodoTaskId = int.Parse(substrings[1].Trim());
        if (substrings.Length >= 5)
        {
            Ordinal = int.Parse(substrings[2].Trim());
            Title = substrings[3].Trim();
            IsCompleted = bool.Parse(substrings[4].Trim());
        }
        else
        {
            Title = substrings[2].Trim();
            IsCompleted = bool.Parse(substrings[3].Trim());
        }
        return this;
    }
}
