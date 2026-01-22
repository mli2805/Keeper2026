using KeeperDomain;

namespace KeeperInfrastructure;

public static class RatesMapper
{
    public static DateTime DenominationDay = new DateTime(2016, 7, 1);

    public static OfficialRatesEf ToEf(this OfficialRates officialRates)
    {
        return new OfficialRatesEf
        {
            Id = officialRates.Id,
            Date = officialRates.Date,
            UsdRate = officialRates.NbRates.Usd.Value,
            EuroRate = officialRates.NbRates.Euro.Value,
            RubRate = officialRates.Date.CompareTo(DenominationDay) < 0 
                ? officialRates.NbRates.Rur.Value 
                : officialRates.NbRates.Rur.Value / 100,
            CnyRate = officialRates.Date.CompareTo(DenominationDay) < 0
                ? officialRates.NbRates.Cny.Value
                : officialRates.NbRates.Cny.Value / 10,
            CbrUsdRate = officialRates.CbrRate.Usd.Value,
        };
    }

    public static OfficialRates FromEf(this OfficialRatesEf officialRatesEf)
    {
        var rubRate = officialRatesEf.Date.CompareTo(DenominationDay) < 0 
            ? new OneRate() { Value = officialRatesEf.RubRate, Unit = 1 }
            : new OneRate() { Value = officialRatesEf.RubRate * 100, Unit = 100 };

        var cnyRate = officialRatesEf.Date.CompareTo(DenominationDay) < 0
            ? new OneRate() { Value = officialRatesEf.CnyRate, Unit = 1 }
            : new OneRate() { Value = officialRatesEf.CnyRate * 10, Unit = 10 };

        return new OfficialRates
        {
            Id = officialRatesEf.Id,
            Date = officialRatesEf.Date,
            NbRates = new NbRbRates
            {
                Usd = new OneRate { Value = officialRatesEf.UsdRate },
                Euro = new OneRate { Value = officialRatesEf.EuroRate },
                Rur = rubRate,
                Cny = cnyRate
            },
            CbrRate = new CbrRate
            {
                Usd = new OneRate { Value = officialRatesEf.CbrUsdRate }
            }
        };
    }


    public static ExchangeRatesEf ToEf(this ExchangeRates exchangeRates)
    {
        return new ExchangeRatesEf
        {
            Id = exchangeRates.Id, // Id генерируется в коде приложения
            Date = exchangeRates.Date,
            BynUsdA = exchangeRates.UsdToByn,
            BynUsdB = exchangeRates.BynToUsd,
            BynEurA = exchangeRates.EurToByn,
            BynEurB = exchangeRates.BynToEur,
            BynRubA = exchangeRates.RubToByn,
            BynRubB = exchangeRates.BynToRub,
            EurUsdA = exchangeRates.EurToUsd,
            EurUsdB = exchangeRates.UsdToEur,
            RubUsdA = exchangeRates.UsdToRub,
            RubUsdB = exchangeRates.RubToUsd,
            RubEurA = exchangeRates.EurToRub,
            RubEurB = exchangeRates.RubToEur
        };
    }
    public static ExchangeRates FromEf(this ExchangeRatesEf exchangeRatesEf)
    {
        return new ExchangeRates
        {
            Id = exchangeRatesEf.Id,
            Date = exchangeRatesEf.Date,
            UsdToByn = exchangeRatesEf.BynUsdA,
            BynToUsd = exchangeRatesEf.BynUsdB,
            EurToByn = exchangeRatesEf.BynEurA,
            BynToEur = exchangeRatesEf.BynEurB,
            RubToByn = exchangeRatesEf.BynRubA,
            BynToRub = exchangeRatesEf.BynRubB,
            EurToUsd = exchangeRatesEf.EurUsdA,
            UsdToEur = exchangeRatesEf.EurUsdB,
            UsdToRub = exchangeRatesEf.RubUsdA,
            RubToUsd = exchangeRatesEf.RubUsdB,
            EurToRub = exchangeRatesEf.RubEurA,
            RubToEur = exchangeRatesEf.RubEurB
        };
    }


    public static MetalRateEf ToEf(this MetalRate metalRate)
    {
        return new MetalRateEf
        {
            Id = metalRate.Id,
            Date = metalRate.Date,
            Metal = metalRate.Metal,
            Proba = metalRate.Proba,
            Price = metalRate.Price
        };
    }

    public static MetalRate FromEf(this MetalRateEf metalRateEf)
    {
        return new MetalRate
        {
            Id = metalRateEf.Id,
            Date = metalRateEf.Date,
            Metal = metalRateEf.Metal,
            Proba = metalRateEf.Proba,
            Price = metalRateEf.Price
        };
    }


    public static RefinancingRateEf ToEf(this RefinancingRate refinancingRate)
    {
        return new RefinancingRateEf
        {
            Id = refinancingRate.Id,
            Date = refinancingRate.Date,
            Value = refinancingRate.Value
        };
    }

    public static RefinancingRate FromEf(this RefinancingRateEf refinancingRateEf)
    {
        return new RefinancingRate
        {
            Id = refinancingRateEf.Id,
            Date = refinancingRateEf.Date,
            Value = refinancingRateEf.Value
        };
    }
}
