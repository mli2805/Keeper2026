using Caliburn.Micro;
using System.Collections.Generic;

namespace KeeperWpf;

public class HouseholdPaymentTemplate : PropertyChangedBase
{
    private bool _isOn = false;
    public bool IsOn { get => _isOn; set { _isOn = value; NotifyOfPropertyChange(() => IsOn); } }
    public AccountItemModel Counterparty { get; set; }
    public AccountItemModel Category { get; set; }

    public string Kor => BuildLine(); // Кому - За что (Тег)
    
    private decimal _amount;
    public decimal Amount { get => _amount; set { _amount = value; NotifyOfPropertyChange(() => Amount); } }
    public decimal AmountForecast { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.BYN;
    public List<AccountItemModel> Tags { get; set; }
    public string Comment { get; set; }

    public HouseholdPaymentTemplate(AccountItemModel counterparty, AccountItemModel category,
        decimal amount = 0m, decimal? amountForecast = null,
        List<AccountItemModel>? tags = null, string comment = "")
    {
        Counterparty = counterparty;
        Category = category;
        Amount = amount;
        AmountForecast = amountForecast ?? amount; // чтобы не писать 2 раза, когда совпадает с Amount
        Tags = tags ?? new List<AccountItemModel>();
        Comment = comment;
    }

    private string BuildLine()
    {
        var tag = Tags.Count == 1 ? $"({Tags[0].Name})" : "";
        return $"{Counterparty.Name} - {Category.Name} {tag}";
    }
}