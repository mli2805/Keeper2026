namespace KeeperDomain;

public class CarYearMileage
{
    public int Id { get; set; } //PK
    public int CarId { get; set; }
    public int Odometer { get; set; }
}
