using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class CarRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<CarModel>> GetAllCarsWithMileages()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var cars = await keeperDbContext.Cars
            .Include(c => c.YearMileages)
            .ToListAsync();

        return cars.Select(carEf =>
        {
            var carModel = new CarModel
            {
                Id = carEf.Id,
                CarAccountId = carEf.CarAccountId,
                Title = carEf.Title,
                IssueYear = carEf.IssueYear,
                Vin = carEf.Vin,
                StateRegNumber = carEf.StateRegNumber,
                PurchaseDate = carEf.PurchaseDate,
                PurchaseMileage = carEf.PurchaseMileage,
                SaleDate = carEf.SaleDate,
                SaleMileage = carEf.SaleMileage,
                SupposedSalePrice = carEf.SupposedSalePrice,
                Comment = carEf.Comment,
                YearsMileage = carEf.YearMileages.Select(m => new YearMileageModel
                {
                    Id = m.Id,
                    CarId = m.CarId,
                    Odometer = m.Odometer
                }).ToList()
            };

            return carModel;
        }).ToList();
    }

    public async Task<List<Car>> GetAllCars()
    {
        using var keeperDbContext = factory.CreateDbContext();
        return keeperDbContext.Cars.Select(c=>c.FromEf()).ToList();
    }

    public async Task<List<CarYearMileage>> GetAllCarYearMileages()
    {
        using var keeperDbContext = factory.CreateDbContext();
        return keeperDbContext.CarYearMileages.Select(m=>m.FromEf()).ToList();
    }

    public async Task AddCar(Car car)
    {
        using var keeperDbContext = factory.CreateDbContext();
        keeperDbContext.Cars.Add(car.ToEf());
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task AddCarYearMileage(CarYearMileage carYearMileage)
    {
        using var keeperDbContext = factory.CreateDbContext();
        keeperDbContext.CarYearMileages.Add(carYearMileage.ToEf());
        await keeperDbContext.SaveChangesAsync();
    }
}
