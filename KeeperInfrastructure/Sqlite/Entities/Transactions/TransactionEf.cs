using KeeperDomain;
using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class TransactionEf
{
    public int Id { get; set; } //PK
    public DateTime Timestamp { get; set; }
    public OperationType Operation { get; set; }
    public PaymentWay PaymentWay { get; set; }
    public int Receipt { get; set; }
    public int MyAccount { get; set; }
    public int? MySecondAccount { get; set; }
    public int? Counterparty { get; set; }
    public int? Category { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public decimal? AmountInReturn { get; set; }
    public CurrencyCode? CurrencyInReturn { get; set; }
    [MaxLength(100)] public string? Tags { get; set; }
    [MaxLength(250)] public string Comment { get; set; } = string.Empty;
}
