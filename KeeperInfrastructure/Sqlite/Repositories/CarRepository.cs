using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class CarRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<CarModel>> GetAllCarsWithMileages()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var cars = await keeperDbContext.Cars
            .Include(c => c.YearMileages)
            .ToListAsync();

        return cars.Select(carEf => carEf.ToModel()).ToList();
    }

    public async Task SaveCarWithMileages(CarModel carModel)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var carEf = await keeperDbContext.Cars.Include(c => c.YearMileages).FirstOrDefaultAsync(c => c.Id == carModel.Id);
        if (carEf == null)
        {
            carEf = carModel.ToEf();
            await keeperDbContext.Cars.AddAsync(carEf);
        }
        else
        {
            carEf.CarAccountId = carModel.CarAccountId;
            carEf.Title = carModel.Title;
            carEf.IssueYear = carModel.IssueYear;
            carEf.Vin = carModel.Vin;
            carEf.StateRegNumber = carModel.StateRegNumber;
            carEf.PurchaseDate = carModel.PurchaseDate;
            carEf.PurchaseMileage = carModel.PurchaseMileage;
            carEf.SaleDate = carModel.SaleDate;
            carEf.SaleMileage = carModel.SaleMileage;
            carEf.SupposedSalePrice = carModel.SupposedSalePrice;
            carEf.Comment = carModel.Comment;
            foreach (var yearMileage in carModel.YearsMileage)
            {
                var yearMileageEf = carEf.YearMileages.FirstOrDefault(m => m.Id == yearMileage.Id);
                if (yearMileageEf == null)
                {
                    yearMileageEf = yearMileage.ToEf();
                    yearMileageEf.CarId = carEf.Id;
                    await keeperDbContext.CarYearMileages.AddAsync(yearMileageEf);
                }
                else
                {
                    yearMileageEf.Odometer = yearMileage.Odometer;
                }
            }
            foreach (var yearMileageEf in carEf.YearMileages)
            {
                if (carModel.YearsMileage.FirstOrDefault(m => m.Id == yearMileageEf.Id) == null)
                {
                    keeperDbContext.CarYearMileages.Remove(yearMileageEf);
                }
            }
        }
        await keeperDbContext.SaveChangesAsync();
    }
}
