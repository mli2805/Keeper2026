namespace KeeperInfrastructure;

public static class CarMapper
{
    public static CarEf ToEf(this KeeperDomain.Car car)
    {
        return new CarEf
        {
            CarAccountId = car.CarAccountId,
            Title = car.Title,
            IssueYear = car.IssueYear,
            Vin = car.Vin,
            StateRegNumber = car.StateRegNumber,
            PurchaseDate = car.PurchaseDate,
            PurchaseMileage = car.PurchaseMileage,
            SaleDate = car.SaleDate,
            SaleMileage = car.SaleMileage,
            SupposedSalePrice = car.SupposedSalePrice,
            Comment = car.Comment
        };
    }

    public static KeeperDomain.Car FromEf(this CarEf carEf)
    {
        return new KeeperDomain.Car
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
            Comment = carEf.Comment
        };
    }

    public static CarYearMileageEf ToEf(this KeeperDomain.CarYearMileage carYearMileage)
    {
        return new CarYearMileageEf
        {
            CarId = carYearMileage.CarId,
            Odometer = carYearMileage.Odometer
        };
    }

    public static KeeperDomain.CarYearMileage FromEf(this CarYearMileageEf carYearMileageEf)
    {
        return new KeeperDomain.CarYearMileage
        {
            Id = carYearMileageEf.Id,
            CarId = carYearMileageEf.CarId,
            Odometer = carYearMileageEf.Odometer
        };
    }
}
