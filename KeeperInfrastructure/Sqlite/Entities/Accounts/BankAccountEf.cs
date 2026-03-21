using KeeperDomain;
using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class BankAccountEf
{
    public int Id { get; set; } // совпадает с ID Account'a

    public int BankId { get; set; }
    public int DepositOfferId { get; set; } // не только по депозиту, но и по карте и даже расчетному счету могут начисляться %%
    public CurrencyCode MainCurrency { get; set; }

    [MaxLength(50)]  public string AgreementNumber { get; set; } = ""; // номер договора, где он есть
    [MaxLength(50)] public string ReplenishDetails { get; set; } = ""; // реквизиты для пополнения
    public bool IsReplenishStopped { get; set; } // перенести сюда из Deposit


    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; } // max для расч счетов

    public bool IsMine { get; set; }
}
