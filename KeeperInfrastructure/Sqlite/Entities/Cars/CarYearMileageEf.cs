namespace KeeperInfrastructure;

public class CarYearMileageEf
{
    public int Id { get; set; } //PK
    public int CarId { get; set; }
    public int Odometer { get; set; }

    // Навигационное свойство обратно к Car
    public CarEf Car { get; set; } = null!;
}
