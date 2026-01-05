namespace KeeperInfrastructure
{
    public class CarYearMileageEf
    {
        public int Id { get; private set; } //PK
        public int CarId { get; set; }
        public int Odometer { get; set; }
    }
}
