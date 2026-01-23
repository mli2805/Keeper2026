using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class FuellingsMapper
{
    public static FuellingEf ToEf(this FuellingModel fuellingModel)
    {
        return new FuellingEf
        {
            Id = fuellingModel.Id,
            TransactionId = fuellingModel.Transaction.Id,
            CarAccountId = fuellingModel.CarAccountId,
            Volume = fuellingModel.Volume,
            FuelType = fuellingModel.FuelType,
        };
    }

    public static FuellingEf ToEf(this Fuelling fuelling)
    {
        return new FuellingEf
        {
            Id = fuelling.Id,
            TransactionId = fuelling.TransactionId,
            CarAccountId = fuelling.CarAccountId,
            Volume = fuelling.Volume,
            FuelType = fuelling.FuelType,
        };
    }

    public static Fuelling FromEf(this FuellingEf fuellingEf)
    {
        return new Fuelling
        {
            Id = fuellingEf.Id,
            TransactionId = fuellingEf.TransactionId,
            CarAccountId = fuellingEf.CarAccountId,
            Volume = fuellingEf.Volume,
            FuelType = fuellingEf.FuelType,
        };
    }
}
