using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class DepositOfferMapper
{
    public static void UpdateEf(this DepositOfferEf depositOfferEf, DepositOfferModel depositOfferModel)
    {
        depositOfferEf.Title = depositOfferModel.Title;
        depositOfferEf.IsNotRevocable = depositOfferModel.IsNotRevocable;
        depositOfferEf.RateType = depositOfferModel.RateType;
        depositOfferEf.IsAddLimited = depositOfferModel.IsAddLimited;
        depositOfferEf.AddLimitInDays = depositOfferModel.AddLimitInDays;
        depositOfferEf.BankId = depositOfferModel.Bank.Id;
        depositOfferEf.MainCurrency = depositOfferModel.MainCurrency;

        depositOfferEf.IsPerpetual = depositOfferModel.DepositTerm?.IsPerpetual ?? true;
        depositOfferEf.DepositTermValue = depositOfferModel.DepositTerm?.Value ?? 0;
        depositOfferEf.DepositTermDuration = depositOfferModel.DepositTerm?.Scale ?? Durations.Days;

        depositOfferEf.MonthPaymentsMinimum = depositOfferModel.MonthPaymentsMinimum;
        depositOfferEf.MonthPaymentsMaximum = depositOfferModel.MonthPaymentsMaximum;
        depositOfferEf.Comment = depositOfferModel.Comment;
    }

    public static void UpdateEf(this DepositConditionsEf depositConditionsEf, DepoCondsModel depoCondsModel)
    {
        depositConditionsEf.DateFrom = depoCondsModel.DateFrom;
        depositConditionsEf.RateFormula = depoCondsModel.RateFormula;
        depositConditionsEf.IsFactDays = depoCondsModel.IsFactDays;
        depositConditionsEf.EveryStartDay = depoCondsModel.EveryStartDay;
        depositConditionsEf.EveryFirstDayOfMonth = depoCondsModel.EveryFirstDayOfMonth;
        depositConditionsEf.EveryLastDayOfMonth = depoCondsModel.EveryLastDayOfMonth;
        depositConditionsEf.EveryNDays = depoCondsModel.EveryNDays;
        depositConditionsEf.NDays = depoCondsModel.NDays;
        depositConditionsEf.IsCapitalized = depoCondsModel.IsCapitalized;
        depositConditionsEf.HasAdditionalPercent = depoCondsModel.HasAdditionalPercent;
        depositConditionsEf.AdditionalPercent = depoCondsModel.AdditionalPercent;
        depositConditionsEf.Comment = depoCondsModel.Comment;
    }

    public static void UpdateEf(this DepositRateLineEf depositRateLineEf, DepositRateLine depositRateLineModel)
    {
        depositRateLineEf.DateFrom = depositRateLineModel.DateFrom;
        depositRateLineEf.AmountFrom = depositRateLineModel.AmountFrom;
        depositRateLineEf.AmountTo = depositRateLineModel.AmountTo;
        depositRateLineEf.Rate = depositRateLineModel.Rate;
    }   

    public static DepositOfferModel ToModel(this DepositOfferEf depositOfferEf, Dictionary<int, AccountItemModel> acMoDict)
    {
        return new DepositOfferModel
        {
            Id = depositOfferEf.Id,
            Title = depositOfferEf.Title,
            IsNotRevocable = depositOfferEf.IsNotRevocable,
            RateType = depositOfferEf.RateType,
            IsAddLimited = depositOfferEf.IsAddLimited,
            AddLimitInDays = depositOfferEf.AddLimitInDays,
            Bank = acMoDict[depositOfferEf.BankId],
            MainCurrency = depositOfferEf.MainCurrency,
            DepositTerm = depositOfferEf.IsPerpetual 
                ? new DurationModel() 
                : new DurationModel(depositOfferEf.DepositTermValue, depositOfferEf.DepositTermDuration),
            MonthPaymentsMinimum = depositOfferEf.MonthPaymentsMinimum,
            MonthPaymentsMaximum = depositOfferEf.MonthPaymentsMaximum,
            CondsMap = depositOfferEf.Conditions.ToDictionary(c => c.DateFrom, c => c.ToModel()),
            Comment = depositOfferEf.Comment
        };
    }

    private static DepoCondsModel ToModel(this DepositConditionsEf depositConditionsEf)
    {
        return new DepoCondsModel
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
            RateLines = depositConditionsEf.RateLines.Select(rlEf => rlEf.FromEf()).ToList(),
            Comment = depositConditionsEf.Comment
        };
    }

    public static DepositOfferEf FromModel(this DepositOfferModel depositOfferModel)
    {
        return new DepositOfferEf
        {
            Id = depositOfferModel.Id,
            Title = depositOfferModel.Title,
            IsNotRevocable = depositOfferModel.IsNotRevocable,
            RateType = depositOfferModel.RateType,
            IsAddLimited = depositOfferModel.IsAddLimited,
            AddLimitInDays = depositOfferModel.AddLimitInDays,
            BankId = depositOfferModel.Bank.Id,
            MainCurrency = depositOfferModel.MainCurrency,
            IsPerpetual = depositOfferModel.DepositTerm?.IsPerpetual ?? true,
            DepositTermValue = depositOfferModel.DepositTerm?.Value ?? 0,
            DepositTermDuration = depositOfferModel.DepositTerm?.Scale ?? Durations.Days,
            MonthPaymentsMinimum = depositOfferModel.MonthPaymentsMinimum,
            MonthPaymentsMaximum = depositOfferModel.MonthPaymentsMaximum,
            Conditions = depositOfferModel.CondsMap.Values.Select(c => c.FromModel()).ToList(),
            Comment = depositOfferModel.Comment
        };
    }

    public static DepositConditionsEf FromModel(this DepoCondsModel depoCondsModel)
    {
        return new DepositConditionsEf
        {
            Id = depoCondsModel.Id,
            DepositOfferId = depoCondsModel.DepositOfferId,
            DateFrom = depoCondsModel.DateFrom,
            RateFormula = depoCondsModel.RateFormula,
            IsFactDays = depoCondsModel.IsFactDays,
            EveryStartDay = depoCondsModel.EveryStartDay,
            EveryFirstDayOfMonth = depoCondsModel.EveryFirstDayOfMonth,
            EveryLastDayOfMonth = depoCondsModel.EveryLastDayOfMonth,
            EveryNDays = depoCondsModel.EveryNDays,
            NDays = depoCondsModel.NDays,
            IsCapitalized = depoCondsModel.IsCapitalized,
            HasAdditionalPercent = depoCondsModel.HasAdditionalPercent,
            AdditionalPercent = depoCondsModel.AdditionalPercent,
            RateLines = depoCondsModel.RateLines.Select(rl => rl.ToEf()).ToList(),
            Comment = depoCondsModel.Comment
        };
    }

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
            DepositTermDuration = depositOffer.DepositTerm?.Scale ?? Durations.Days,
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
                : new Duration(depositOfferEf.DepositTermValue, depositOfferEf.DepositTermDuration),
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
