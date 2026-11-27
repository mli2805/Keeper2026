namespace KeeperDomain;

public class SalaryPayment
{
    public int Id { get; set; }
    public int EmployerId { get; set; }
    public int Day { get; set; }
    public int Category { get; set; }
    public decimal Amount { get; set; }
}
