using System.ComponentModel.DataAnnotations;

namespace KeeperDomain;

public class Account
{
    public int Id { get; set; }
    [MaxLength(50)] public string Name { get; set; } = null!;
    public int ParentId { get; set; }
    public int ChildNumber { get; set; } // какой по счету ребенок у родителя

    public bool IsFolder;
    public bool IsExpanded;

    public int AssociatedIncomeId { get; set; } // for counterparty
    public int AssociatedExpenseId { get; set; } // for counterparty
    public int AssociatedExternalId { get; set; } // for category
    public int AssociatedTagId { get; set; } // for counterparty or category
    public int MCC { get; set; } // for counterparty

    [MaxLength(20)] public string ShortName { get; set; } = string.Empty; // short name for reports
    [MaxLength(5)] public string ButtonName { get; set; } = string.Empty; // face of shortcut button (if exists)

    [MaxLength(100)] public string Comment { get; set; } = string.Empty;
}
