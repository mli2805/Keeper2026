using System;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public class CardOrAccountVm
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public bool IsMine { get; set; } // 0 - mine, 1 - julia

    public PaymentSystem PaymentSystem { get; set; } = PaymentSystem.CurrentAccount; // will be changed later, if necessary
    public string PaymentSystemStr => PaymentSystem == PaymentSystem.CurrentAccount ? " текущий счет" : PaymentSystem.ToString();
    public bool IsVirtual { get; set; }
    public bool IsPayPass { get; set; }

    public string AgreementNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }

    public AccountItemModel AccountItemOfBank { get; set; } = null!;
    public CurrencyCode MainCurrency { get; set; }

    public string Name { get; set; } = null!;

    public decimal Amount { get; set; }
}
