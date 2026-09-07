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
    public TodoCategory Category { get; set; } = TodoCategory.CountryHouse;

    public string Dump()
    {
        var completedAt = CompletedAt.HasValue 
            ? CompletedAt.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty;
        return Id + " ; " + Title.Replace("\r\n", "|") + " ; " + 
            CreatedAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " ; " + completedAt + " ; " +
            Importance + " ; " + Category;
    }

    public TodoTask FromString(string s)
    {
        var substrings = s.Split(';');

        Id = int.Parse(substrings[0].Trim());
        Title = substrings[1].Trim().Replace("|", "\r\n");
        CreatedAt = DateOnly.ParseExact(substrings[2].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var completedAt = substrings[3].Trim();
        CompletedAt = string.IsNullOrWhiteSpace(completedAt)
            ? null
            : DateOnly.ParseExact(completedAt, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        Importance = Enum.Parse<TodoImportance>(substrings[4].Trim());
        Category = substrings.Length > 5 && !string.IsNullOrWhiteSpace(substrings[5])
            ? Enum.Parse<TodoCategory>(substrings[5].Trim())
            : TodoCategory.CountryHouse;
        return this;
    }
}
