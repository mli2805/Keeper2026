using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class BankAccountMemoEf
{
    public int Id { get; set; }
    public int AccountId { get; set; }

    public int? BalanceLess { get; set; }
    public int? BalanceMore { get; set; }
    public int? PaymentsLess { get; set; }
    public int? PaymentsMore { get; set; }

    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;
}