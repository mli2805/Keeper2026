using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class OthersMapper
{
    public static LargeExpenseThresholdEf ToEf(this LargeExpenseThreshold domain)
    {
        return new LargeExpenseThresholdEf
        {
            Id = domain.Id,
            FromDate = domain.FromDate,
            Amount = domain.Amount,
            AmountForYearAnalysis = domain.AmountForYearAnalysis
        };
    }

    public static LargeExpenseThreshold FromEf(this LargeExpenseThresholdEf ef)
    {
        return new LargeExpenseThreshold
        {
            Id = ef.Id,
            FromDate = ef.FromDate,
            Amount = ef.Amount,
            AmountForYearAnalysis = ef.AmountForYearAnalysis
        };
    }

    public static CardBalanceMemoEf ToEf(this CardBalanceMemo domain)
    {
        return new CardBalanceMemoEf
        {
            Id = domain.Id,
            AccountId = domain.AccountId,
            BalanceThreshold = domain.BalanceThreshold
        };
    }

    public static CardBalanceMemo FromEf(this CardBalanceMemoEf ef)
    {
        return new CardBalanceMemo
        {
            Id = ef.Id,
            AccountId = ef.AccountId,
            BalanceThreshold = ef.BalanceThreshold
        };
    }

    public static SalaryChangeEf ToEf(this SalaryChange domain)
    {
        return new SalaryChangeEf
        {
            Id = domain.Id,
            EmployerId = domain.EmployerId,
            FirstReceived = domain.FirstReceived,
            Amount = domain.Amount,
            Comment = domain.Comment
        };
    }

    public static SalaryChange FromEf(this SalaryChangeEf ef)
    {
        return new SalaryChange
        {
            Id = ef.Id,
            EmployerId = ef.EmployerId,
            FirstReceived = ef.FirstReceived,
            Amount = ef.Amount,
            Comment = ef.Comment
        };
    }

    public static SalaryPaymentEf ToEf(this SalaryPayment domain)
    {
        return new SalaryPaymentEf
        {
            Id = domain.Id,
            EmployerId = domain.EmployerId,
            Day = domain.Day,
            Category = domain.Category,
            Amount = domain.Amount
        };
    }

    public static SalaryPayment FromEf(this SalaryPaymentEf ef)
    {
        return new SalaryPayment
        {
            Id = ef.Id,
            EmployerId = ef.EmployerId,
            Day = ef.Day,
            Category = ef.Category,
            Amount = ef.Amount
        };
    }

    public static ButtonCollectionEf ToEf(this ButtonCollection bc)
    {
        return new ButtonCollectionEf
        {
            Id = bc.Id,
            Name = bc.Name,
            AccountIdsString = bc.AccountIds.Count > 0 ? string.Join(";", bc.AccountIds) : ""
        };
    }

    public static ButtonCollectionEf ToEf(this ButtonCollectionModel bcm)
    {
        return new ButtonCollectionEf
        {
            Id = bcm.Id,
            Name = bcm.Name,
            AccountIdsString = bcm.AccountModels.Count > 0 ? string.Join(";", bcm.AccountModels.Select(am => am.Id)) : ""
        };
    }

    public static ButtonCollection FromEf(this ButtonCollectionEf bcEf)
    {
        return new ButtonCollection
        {
            Id = bcEf.Id,
            Name = bcEf.Name,
            AccountIds = !string.IsNullOrEmpty(bcEf.AccountIdsString)
                ? bcEf.AccountIdsString.Split(';').Select(s => int.Parse(s.Trim())).ToList()
                : new List<int>()
        };
    }

    public static UtilityMetersEf ToEf(this UtilityMeters domain)
    {
        return new UtilityMetersEf
        {
            Id = domain.Id,
            ReadingsDate = domain.ReadingsDate,
            ColdWaterFrom = domain.ColdWater.From,
            ColdWaterTo = domain.ColdWater.To,
            ColdWaterVolume = domain.ColdWater.Volume,
            ColdWaterTariff = domain.ColdWater.Tariff,
            HotWaterFrom = domain.HotWater.From,
            HotWaterTo = domain.HotWater.To,
            HotWaterVolume = domain.HotWater.Volume,
            HotWaterTariff = domain.HotWater.Tariff,
            ElectricityFrom = domain.Electricity.From,
            ElectricityTo = domain.Electricity.To,
            ElectricityVolume = domain.Electricity.Volume,
            ElectricityTariff = domain.Electricity.Tariff,
            GasFrom = domain.Gas.From,
            GasTo = domain.Gas.To,
            GasVolume = domain.Gas.Volume,
            GasTariff = domain.Gas.Tariff
        };
    }

    public static UtilityMetersEf ToEf(this UtilityMetersModel model)
    {
        return new UtilityMetersEf
        {
            Id = model.Id,
            ReadingsDate = DateOnly.FromDateTime(model.ReadingsDate),
            ColdWaterFrom = model.ColdWater.From,
            ColdWaterTo = model.ColdWater.To,
            ColdWaterVolume = model.ColdWater.Volume,
            ColdWaterTariff = model.ColdWater.Tariff,
            HotWaterFrom = model.HotWater.From,
            HotWaterTo = model.HotWater.To,
            HotWaterVolume = model.HotWater.Volume,
            HotWaterTariff = model.HotWater.Tariff,
            ElectricityFrom = model.Electricity.From,
            ElectricityTo = model.Electricity.To,
            ElectricityVolume = model.Electricity.Volume,
            ElectricityTariff = model.Electricity.Tariff,
            GasFrom = model.Gas.From,
            GasTo = model.Gas.To,
            GasVolume = model.Gas.Volume,
            GasTariff = model.Gas.Tariff
        };
    }

    public static UtilityMeters FromEf(this UtilityMetersEf ef)
    {
        return new UtilityMeters
        {
            Id = ef.Id,
            ReadingsDate = ef.ReadingsDate,
            ColdWater = new UtilityMeter 
            { 
                From = ef.ColdWaterFrom, To = ef.ColdWaterTo, Volume = ef.ColdWaterVolume, Tariff = ef.ColdWaterTariff
            },
            HotWater = new UtilityMeter 
            { 
                From = ef.HotWaterFrom, To = ef.HotWaterTo, Volume = ef.HotWaterVolume, Tariff = ef.HotWaterTariff
            },
            Electricity = new UtilityMeter 
            { 
                From = ef.ElectricityFrom, To = ef.ElectricityTo, Volume = ef.ElectricityVolume, Tariff = ef.ElectricityTariff
            },
            Gas = new UtilityMeter 
            { 
                From = ef.GasFrom, To = ef.GasTo, Volume = ef.GasVolume, Tariff = ef.GasTariff
            }
        };
    }

    public static UtilityMetersModel ToModel(this UtilityMetersEf ef)
    {
        return new UtilityMetersModel
        {
            Id = ef.Id,
            ReadingsDate = ef.ReadingsDate.ToDateTime(TimeOnly.MinValue),
            ColdWater = new UtilityMeterModel
            {
                From = ef.ColdWaterFrom, To = ef.ColdWaterTo, Volume = ef.ColdWaterVolume, Tariff = ef.ColdWaterTariff
            },
            HotWater = new UtilityMeterModel
            {
                From = ef.HotWaterFrom, To = ef.HotWaterTo, Volume = ef.HotWaterVolume, Tariff = ef.HotWaterTariff
            },
            Electricity = new UtilityMeterModel
            {
                From = ef.ElectricityFrom, To = ef.ElectricityTo, Volume = ef.ElectricityVolume, Tariff = ef.ElectricityTariff
            },
            Gas = new UtilityMeterModel
            {
                From = ef.GasFrom, To = ef.GasTo, Volume = ef.GasVolume, Tariff = ef.GasTariff
            }
        };
    }
}