namespace KeeperInfrastructure;

public class LargeExpenseThresholdEf
{
    public int Id { get; set; }
    public DateTime FromDate { get; set; }
    public decimal Amount { get; set; } // for month analysis
    public decimal AmountForYearAnalysis { get; set; }
}
