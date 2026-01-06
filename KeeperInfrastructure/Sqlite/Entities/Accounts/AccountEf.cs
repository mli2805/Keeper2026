using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class AccountEf
{
    public int Id { get; set; }
    [MaxLength(50)] public string Name { get; set; }
    public int ParentId { get; set; }

    public bool IsFolder;
    public bool IsExpanded;

    public int AssociatedIncomeId { get; set; } // for counterparty
    public int AssociatedExpenseId { get; set; } // for counterparty
    public int AssociatedExternalId { get; set; } // for category
    public int AssociatedTagId { get; set; } // for counterparty or category

    [MaxLength(20)] public string ShortName { get; set; }
    [MaxLength(5)] public string ButtonName { get; set; } // face of shortcut button (if exists)

    [MaxLength(100)] public string Comment { get; set; }
}
