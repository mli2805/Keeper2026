using KeeperDomain; 

namespace KeeperInfrastructure
{
    public class CarRepository(KeeperDbContext keeperDbContext)
    {
        public async Task<List<Car>> GetAllCars()
        {
            return keeperDbContext.Cars.Select(c=>c.FromEf()).ToList();
        }

        public async Task<List<CarYearMileage>> GetAllCarYearMileages()
        {
            return keeperDbContext.CarYearMileages.Select(m=>m.FromEf()).ToList();
        }

        public async Task AddCar(Car car)
        {
            keeperDbContext.Cars.Add(car.ToEf());
            await keeperDbContext.SaveChangesAsync();
        }

        public async Task AddCarYearMileage(CarYearMileage carYearMileage)
        {
            keeperDbContext.CarYearMileages.Add(carYearMileage.ToEf());
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
