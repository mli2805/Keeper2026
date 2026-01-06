using System.ComponentModel.DataAnnotations;
using KeeperDomain;

namespace KeeperInfrastructure;

public class DepositOfferEf
{
    public int Id { get; set; }
    [MaxLength(100)] public string Title { get; set; }
    public bool IsNotRevocable { get; set; }
    public RateType RateType { get; set; }
    public bool IsAddLimited { get; set; }
    public int AddLimitInDays { get; set; }

    public int BankId { get; set; }
    public CurrencyCode MainCurrency { get; set; }

    public bool IsPerpetual { get; set; }
    public int DepositTermValue { get; set; }
    public Durations DepositTermDurations { get; set; }

    public int MonthPaymentsMinimum { get; set; }
    public int MonthPaymentsMaximum { get; set; }

    [MaxLength(250)] public string Comment { get; set; }
}