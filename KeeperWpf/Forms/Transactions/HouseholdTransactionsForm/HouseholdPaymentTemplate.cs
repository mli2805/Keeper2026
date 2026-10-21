using System.Collections.Generic;

namespace KeeperWpf;

public class HouseholdPaymentTemplate
{
    public bool IsOn { get; set; } = true;
    public AccountItemModel Counterparty { get; set; }
    public AccountItemModel Category { get; set; }

    public string Kor => BuildLine();
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.BYN;
    public List<AccountItemModel> Tags { get; set; } = new List<AccountItemModel>();
    public string Comment { get; set; }

    public HouseholdPaymentTemplate(AccountItemModel counterparty, AccountItemModel category,
        List<AccountItemModel> tags, string comment = "")
    {
        Counterparty = counterparty;
        Category = category;
        Tags = tags;
        Comment = comment;
    }

    private string BuildLine()
    {
        var tag = Tags.Count == 1 ? $"({Tags[0].Name})" : "";
        return $"{Counterparty.Name} - {Category.Name} {tag}";
    }
}