using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class SalaryChangeEf
{
    public int Id { get; set; }
    public int EmployerId { get; set; }
    public DateTime FirstReceived { get; set; } = new DateTime(2008, 6, 1);
    public decimal Amount { get; set; }
    [MaxLength(250)] public string Comment { get; set; } = string.Empty;
}
