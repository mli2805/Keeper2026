using System.Globalization;

namespace KeeperDomain;

[Serializable]
public class SalaryPayment : IDumpable, IParsable<SalaryPayment>
{
    public int Id { get; set; }
    public int EmployerId { get; set; }
    public int Day { get; set; }
    public int Category { get; set; }
    public decimal Amount { get; set; }

    public string Dump()
    {
        return Id + " ; " + EmployerId + " ; " + Day + " ; " + Category + " ; " +
               Amount.ToString(new CultureInfo("en-US"));
    }

    public SalaryPayment FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        EmployerId = int.Parse(substrings[1]);
        Day = int.Parse(substrings[2]);
        Category = int.Parse(substrings[3]);
        Amount = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));
        return this;
    }
}
