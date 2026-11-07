using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KeeperDomain;

[Serializable]
public class Transaction : IDumpable, IParsable<Transaction>
{
    // Кешируем CultureInfo на уровне класса
    private static readonly CultureInfo _enUsCulture = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;

    public int Id { get; set; } //PK
    public DateTime Timestamp { get; set; }
    public OperationType Operation { get; set; }
    public PaymentWay PaymentWay { get; set; }
    public int Receipt { get; set; }
    public int MyAccount { get; set; }
    public int MySecondAccount { get; set; }
    public int Counterparty { get; set; }
    public int MCC { get; set; }
    public int Category { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public decimal AmountInReturn { get; set; }
    public CurrencyCode? CurrencyInReturn { get; set; }
    [MaxLength(100)] public string Tags { get; set; } = string.Empty;
    [MaxLength(250)] public string Comment { get; set; } = string.Empty;

    public string Dump()
    {
        return Id + " ; " +
               Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
               Operation + " ; " + PaymentWay + " ; " + Receipt + " ; " +
               MyAccount + " ; " + MySecondAccount + " ; " +
               Counterparty + " ; " + MCC + " ; " + Category + " ; " +
               Amount.ToString(_enUsCulture) + " ; " + Currency + " ; " +
               AmountInReturn.ToString(_enUsCulture) + " ; " + CurrencyInReturn + " ; " +
               Tags + " ; " + Comment.Replace("\r\n", "|");
    }

    public Transaction FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0].Trim());
        Timestamp = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy HH:mm", _invariantCulture);
        Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[2]);
        PaymentWay = (PaymentWay)Enum.Parse(typeof(PaymentWay), substrings[3]);
        Receipt = int.Parse(substrings[4].Trim());
        MyAccount = int.Parse(substrings[5].Trim());
        MySecondAccount = int.Parse(substrings[6].Trim());
        Counterparty = int.Parse(substrings[7].Trim());
        MCC = int.Parse(substrings[8].Trim());
        Category = int.Parse(substrings[9].Trim());
        Amount = Convert.ToDecimal(substrings[10], _enUsCulture);
        Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[11]);
        AmountInReturn = Convert.ToDecimal(substrings[12], _enUsCulture);
        CurrencyInReturn = substrings[13].Trim() != ""
            ? (CurrencyCode?)Enum.Parse(typeof(CurrencyCode), substrings[13])
            : null;
        Tags = substrings[14].Trim();
        Comment = substrings[15].Trim().Replace("|", "\r\n");
        return this;
    }
}
