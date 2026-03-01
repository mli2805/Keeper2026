using KeeperModels;

namespace KeeperInfrastructure;

public static class CarMapper
{
    public static CarEf ToEf(this KeeperDomain.Car car)
    {
        return new CarEf
        {
            Id = car.Id,
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

    public static CarYearMileageEf ToEf(this KeeperDomain.CarYearMileage carYearMileage)
    {
        return new CarYearMileageEf
        {
            Id = carYearMileage.Id,
            CarId = carYearMileage.CarId,
            Odometer = carYearMileage.Odometer
        };
    }

    public static CarModel ToModel(this CarEf carEf)
    {
        return new CarModel
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
    }

    public static CarEf ToEf(this CarModel carModel)
    {
        return new CarEf
        {
            Id = carModel.Id,
            CarAccountId = carModel.CarAccountId,
            Title = carModel.Title,
            IssueYear = carModel.IssueYear,
            Vin = carModel.Vin,
            StateRegNumber = carModel.StateRegNumber,
            PurchaseDate = carModel.PurchaseDate,
            PurchaseMileage = carModel.PurchaseMileage,
            SaleDate = carModel.SaleDate,
            SaleMileage = carModel.SaleMileage,
            SupposedSalePrice = carModel.SupposedSalePrice,
            Comment = carModel.Comment,
            YearMileages = carModel.YearsMileage.Select(m => m.ToEf()).ToList()
        };
    }

    public static CarYearMileageEf ToEf (this YearMileageModel yearMileageModel)
    {
        return new CarYearMileageEf
        {
            Id = yearMileageModel.Id,
            CarId = yearMileageModel.CarId,
            Odometer = yearMileageModel.Odometer
        };
    }

    //public static KeeperDomain.Car FromEf(this CarEf carEf)
    //{
    //    return new KeeperDomain.Car
    //    {
    //        Id = carEf.Id,
    //        CarAccountId = carEf.CarAccountId,
    //        Title = carEf.Title,
    //        IssueYear = carEf.IssueYear,
    //        Vin = carEf.Vin,
    //        StateRegNumber = carEf.StateRegNumber,
    //        PurchaseDate = carEf.PurchaseDate,
    //        PurchaseMileage = carEf.PurchaseMileage,
    //        SaleDate = carEf.SaleDate,
    //        SaleMileage = carEf.SaleMileage,
    //        SupposedSalePrice = carEf.SupposedSalePrice,
    //        Comment = carEf.Comment
    //    };
    //}

    //public static KeeperDomain.CarYearMileage FromEf(this CarYearMileageEf carYearMileageEf)
    //{
    //    return new KeeperDomain.CarYearMileage
    //    {
    //        Id = carYearMileageEf.Id,
    //        CarId = carYearMileageEf.CarId,
    //        Odometer = carYearMileageEf.Odometer
    //    };
    //}
}
