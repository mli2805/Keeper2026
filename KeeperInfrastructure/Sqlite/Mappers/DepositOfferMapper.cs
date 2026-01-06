using KeeperDomain;

namespace KeeperInfrastructure;

public static class DepositOfferMapper
{
    public static DepositOfferEf ToEf(this DepositOffer depositOffer)
    {
        return new DepositOfferEf
        {
            Id = depositOffer.Id,
            Title = depositOffer.Title,
            IsNotRevocable = depositOffer.IsNotRevocable,
            RateType = depositOffer.RateType,
            IsAddLimited = depositOffer.IsAddLimited,
            AddLimitInDays = depositOffer.AddLimitInDays,
            BankId = depositOffer.BankId,
            MainCurrency = depositOffer.MainCurrency,
            IsPerpetual = depositOffer.DepositTerm?.IsPerpetual ?? true,
            DepositTermValue = depositOffer.DepositTerm?.Value ?? 0,
            DepositTermDurations = depositOffer.DepositTerm?.Scale ?? Durations.Days,
            MonthPaymentsMinimum = depositOffer.MonthPaymentsMinimum,
            MonthPaymentsMaximum = depositOffer.MonthPaymentsMaximum,
            Comment = depositOffer.Comment
        };
    }

    public static DepositOffer FromEf(this DepositOfferEf depositOfferEf)
    {
        return new DepositOffer
        {
            Id = depositOfferEf.Id,
            Title = depositOfferEf.Title,
            IsNotRevocable = depositOfferEf.IsNotRevocable,
            RateType = depositOfferEf.RateType,
            IsAddLimited = depositOfferEf.IsAddLimited,
            AddLimitInDays = depositOfferEf.AddLimitInDays,
            BankId = depositOfferEf.BankId,
            MainCurrency = depositOfferEf.MainCurrency,
            DepositTerm = depositOfferEf.IsPerpetual 
                ? new Duration() 
                : new Duration(depositOfferEf.DepositTermValue, depositOfferEf.DepositTermDurations),
            MonthPaymentsMinimum = depositOfferEf.MonthPaymentsMinimum,
            MonthPaymentsMaximum = depositOfferEf.MonthPaymentsMaximum,
            Comment = depositOfferEf.Comment
        };
    }

    public static DepositConditionsEf ToEf(this DepositConditions depositConditions)
    {
        return new DepositConditionsEf
        {
            Id = depositConditions.Id,
            DepositOfferId = depositConditions.DepositOfferId,
            DateFrom = depositConditions.DateFrom,
            RateFormula = depositConditions.RateFormula,
            IsFactDays = depositConditions.IsFactDays,
            EveryStartDay = depositConditions.EveryStartDay,
            EveryFirstDayOfMonth = depositConditions.EveryFirstDayOfMonth,
            EveryLastDayOfMonth = depositConditions.EveryLastDayOfMonth,
            EveryNDays = depositConditions.EveryNDays,
            NDays = depositConditions.NDays,
            IsCapitalized = depositConditions.IsCapitalized,
            HasAdditionalPercent = depositConditions.HasAdditionalPercent,
            AdditionalPercent = depositConditions.AdditionalPercent,
            Comment = depositConditions.Comment
        };
    }

    public static DepositConditions FromEf(this DepositConditionsEf depositConditionsEf)
    {
        return new DepositConditions
        {
            Id = depositConditionsEf.Id,
            DepositOfferId = depositConditionsEf.DepositOfferId,
            DateFrom = depositConditionsEf.DateFrom,
            RateFormula = depositConditionsEf.RateFormula,
            IsFactDays = depositConditionsEf.IsFactDays,
            EveryStartDay = depositConditionsEf.EveryStartDay,
            EveryFirstDayOfMonth = depositConditionsEf.EveryFirstDayOfMonth,
            EveryLastDayOfMonth = depositConditionsEf.EveryLastDayOfMonth,
            EveryNDays = depositConditionsEf.EveryNDays,
            NDays = depositConditionsEf.NDays,
            IsCapitalized = depositConditionsEf.IsCapitalized,
            HasAdditionalPercent = depositConditionsEf.HasAdditionalPercent,
            AdditionalPercent = depositConditionsEf.AdditionalPercent,
            Comment = depositConditionsEf.Comment
        };
    }

    public static DepositRateLineEf ToEf(this DepositRateLine depositRateLine)
    {
        return new DepositRateLineEf
        {
            Id = depositRateLine.Id,
            DepositOfferConditionsId = depositRateLine.DepositOfferConditionsId,
            DateFrom = depositRateLine.DateFrom,
            AmountFrom = depositRateLine.AmountFrom,
            AmountTo = depositRateLine.AmountTo,
            Rate = depositRateLine.Rate
        };
    }

    public static DepositRateLine FromEf(this DepositRateLineEf depositRateLineEf)
    {
        return new DepositRateLine
        {
            Id = depositRateLineEf.Id,
            DepositOfferConditionsId = depositRateLineEf.DepositOfferConditionsId,
            DateFrom = depositRateLineEf.DateFrom,
            AmountFrom = depositRateLineEf.AmountFrom,
            AmountTo = depositRateLineEf.AmountTo,
            Rate = depositRateLineEf.Rate
        };
    }
}
