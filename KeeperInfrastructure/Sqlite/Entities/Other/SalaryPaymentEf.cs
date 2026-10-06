namespace KeeperInfrastructure;

public class SalaryPaymentEf
{
    public int Id { get; set; }
    public int EmployerId { get; set; } // 443 - ИИТ, 172 - Оптиксофт
    public int Day { get; set; }
    public int Category { get; set; } // 204 - зарплата, 1008 - аванс
    public decimal Amount { get; set; }
}
