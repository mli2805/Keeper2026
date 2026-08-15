using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KeeperDomain;

public class TodoTask : IDumpable, IParsable<TodoTask>
{
    public int Id { get; set; }
    [MaxLength(1000)] public string Title { get; set; } = string.Empty;
    public DateOnly CreatedAt { get; set; }
    public DateOnly? CompletedAt { get; set; }
    public TodoImportance Importance { get; set; } = TodoImportance.Normal;

    public string Dump()
    {
        var completedAt = CompletedAt.HasValue ? CompletedAt.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty;
        return Id + " ; " + Title + " ; " + CreatedAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " ; "
               + completedAt + " ; " + Importance;
    }

    public TodoTask FromString(string s)
    {
        var substrings = s.Split(';');

        Id = int.Parse(substrings[0].Trim());
        Title = substrings[1].Trim();
        CreatedAt = DateOnly.ParseExact(substrings[2].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var completedAt = substrings[3].Trim();
        CompletedAt = string.IsNullOrWhiteSpace(completedAt)
            ? null
            : DateOnly.ParseExact(completedAt, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        Importance = Enum.Parse<TodoImportance>(substrings[4].Trim());
        return this;
    }
}
