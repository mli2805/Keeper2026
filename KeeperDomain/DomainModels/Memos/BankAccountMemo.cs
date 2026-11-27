namespace KeeperDomain;

public class BankAccountMemo
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int? BalanceLess { get; set; }
    public int? BalanceMore { get; set; }
    public int? PaymentsLess { get; set; }
    public int? PaymentsMore { get; set; }
    public string Comment { get; set; } = string.Empty;
}