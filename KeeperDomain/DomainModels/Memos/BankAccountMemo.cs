using System.Globalization;

namespace KeeperDomain;

public class BankAccountMemo : IDumpable, IParsable<BankAccountMemo>
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int? BalanceLess { get; set; }
    public int? BalanceMore { get; set; }
    public int? PaymentsLess { get; set; }
    public int? PaymentsMore { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Dump()
    {
        return Id + " ; " + AccountId + " ; " +
               BalanceLess?.ToString(new CultureInfo("en-US")) + " ; " +
               BalanceMore?.ToString(new CultureInfo("en-US")) + " ; " +
               PaymentsLess?.ToString(new CultureInfo("en-US")) + " ; " +
               PaymentsMore?.ToString(new CultureInfo("en-US")) + " ; " +
               Comment;
    }
    public BankAccountMemo FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        AccountId = int.Parse(substrings[1]);
        BalanceLess = !string.IsNullOrWhiteSpace(substrings[2]) ? Convert.ToInt32(substrings[2], new CultureInfo("en-US")) : null;
        BalanceMore = !string.IsNullOrWhiteSpace(substrings[3]) ? Convert.ToInt32(substrings[3], new CultureInfo("en-US")) : null;
        PaymentsLess = !string.IsNullOrWhiteSpace(substrings[4]) ? Convert.ToInt32(substrings[4], new CultureInfo("en-US")) : null;
        PaymentsMore = !string.IsNullOrWhiteSpace(substrings[5]) ? Convert.ToInt32(substrings[5], new CultureInfo("en-US")) : null;
        Comment = substrings[6].Trim();
        return this;
    }
}