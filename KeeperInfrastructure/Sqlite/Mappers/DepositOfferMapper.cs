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

        depositOfferEf.IsPerpetual = depositOfferModel.DepositTerm.IsPerpetual;
        depositOfferEf.DepositTermValue = depositOfferModel.DepositTerm.Value;
        depositOfferEf.DepositTermDuration = depositOfferModel.DepositTerm.Scale;

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

    public static void UpdateEf(this MoneyBackRulesEf moneyBackRulesEf, MoneyBackRulesModel moneyBackRulesModel)
    {
        moneyBackRulesEf.StartDate = moneyBackRulesModel.StartDate;
        moneyBackRulesEf.MoneyBackType = moneyBackRulesModel.MoneyBackType;
        moneyBackRulesEf.Percent = moneyBackRulesModel.Percent;
        moneyBackRulesEf.MinPurchaseAmount = moneyBackRulesModel.MinPurchaseAmount;
        moneyBackRulesEf.MaxPurchaseAmount = moneyBackRulesModel.MaxPurchaseAmount;
        moneyBackRulesEf.MaxMoneyBackAmount = moneyBackRulesModel.MaxMoneyBackAmount;
        moneyBackRulesEf.MinMonthPurchasesAmount = moneyBackRulesModel.MinMonthPurchasesAmount;
        moneyBackRulesEf.Comment = moneyBackRulesModel.Comment;
    }

    public static void UpdateEf(this MoneyBackCategoryEf moneyBackCategoryEf, MoneyBackSpecificCategoryModel moneyBackSpecificRuleModel)
    {
        moneyBackCategoryEf.MCC = moneyBackSpecificRuleModel.MCC;
        moneyBackCategoryEf.Name = moneyBackSpecificRuleModel.Name;
        moneyBackCategoryEf.Description = moneyBackSpecificRuleModel.Description;
        moneyBackCategoryEf.Percent = moneyBackSpecificRuleModel.Percent;
    }

    public static void UpdateEf(this MoneyBackPartnerEf moneyBackPartnerEf, MoneyBackPartnerModel moneyBackPartnerModel)
    {
        moneyBackPartnerEf.Name = moneyBackPartnerModel.Name;
        moneyBackPartnerEf.Percent = moneyBackPartnerModel.Percent;
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
            CondsList = depositOfferEf.Conditions.Select(c => c.ToModel()).OrderBy(c => c.DateFrom).ToList(),
            MoneyBackRules = depositOfferEf.MoneyBackRules?.Select(m => m.ToModel()).OrderBy(m => m.StartDate).ToList() ?? new List<MoneyBackRulesModel>(),
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

    public static MoneyBackRulesModel ToModel(this MoneyBackRulesEf moneyBackRulesEf)
    {
        return new MoneyBackRulesModel
        {
            Id = moneyBackRulesEf.Id,
            DepositOfferId = moneyBackRulesEf.DepositOfferId,
            StartDate = moneyBackRulesEf.StartDate,
            MoneyBackType = moneyBackRulesEf.MoneyBackType,
            Percent = moneyBackRulesEf.Percent,
            SpecificCategories = moneyBackRulesEf.SpecificCategories?.Select(sc => sc.ToModel()).ToList() ?? new List<MoneyBackSpecificCategoryModel>(),
            Partners = moneyBackRulesEf.Partners?.Select(p => p.ToModel()).ToList() ?? new List<MoneyBackPartnerModel>(),
            MinPurchaseAmount = moneyBackRulesEf.MinPurchaseAmount,
            MaxPurchaseAmount = moneyBackRulesEf.MaxPurchaseAmount,
            MaxMoneyBackAmount = moneyBackRulesEf.MaxMoneyBackAmount,
            MinMonthPurchasesAmount = moneyBackRulesEf.MinMonthPurchasesAmount,
            Comment = moneyBackRulesEf.Comment
        };
    }

    public static MoneyBackSpecificCategoryModel ToModel(this MoneyBackCategoryEf moneyBackCategoryEf)
    {
        return new MoneyBackSpecificCategoryModel
        {
            Id = moneyBackCategoryEf.Id,
            MoneyBackRulesId = moneyBackCategoryEf.MoneyBackRulesId,
            MCC = moneyBackCategoryEf.MCC,
            Name = moneyBackCategoryEf.Name,
            Description = moneyBackCategoryEf.Description,
            Percent = moneyBackCategoryEf.Percent
        };
    }

    public static MoneyBackPartnerModel ToModel(this MoneyBackPartnerEf moneyBackPartnerEf)
    {
        return new MoneyBackPartnerModel
        {
            Id = moneyBackPartnerEf.Id,
            MoneyBackRulesId = moneyBackPartnerEf.MoneyBackRulesId,
            Name = moneyBackPartnerEf.Name,
            Percent = moneyBackPartnerEf.Percent
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
            IsPerpetual = depositOfferModel.DepositTerm.IsPerpetual,
            DepositTermValue = depositOfferModel.DepositTerm.Value,
            DepositTermDuration = depositOfferModel.DepositTerm.Scale,
            Conditions = depositOfferModel.CondsList.Select(c => c.FromModel()).ToList(),
            MoneyBackRules = depositOfferModel.MoneyBackRules.Select(m => m.FromModel()).ToList(),
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

    public static MoneyBackRulesEf FromModel(this MoneyBackRulesModel moneyBackRulesModel)
    {
        return new MoneyBackRulesEf
        {
            Id = moneyBackRulesModel.Id,
            DepositOfferId = moneyBackRulesModel.DepositOfferId,
            StartDate = moneyBackRulesModel.StartDate,
            MoneyBackType = moneyBackRulesModel.MoneyBackType,
            Percent = moneyBackRulesModel.Percent,
            SpecificCategories = moneyBackRulesModel.SpecificCategories.Select(sc => sc.FromModel()).ToList(),
            Partners = moneyBackRulesModel.Partners.Select(p => p.FromModel()).ToList(),
            MinPurchaseAmount = moneyBackRulesModel.MinPurchaseAmount,
            MaxPurchaseAmount = moneyBackRulesModel.MaxPurchaseAmount,
            MaxMoneyBackAmount = moneyBackRulesModel.MaxMoneyBackAmount,
            MinMonthPurchasesAmount = moneyBackRulesModel.MinMonthPurchasesAmount,
            Comment = moneyBackRulesModel.Comment
        };
    }

    public static MoneyBackCategoryEf FromModel(this MoneyBackSpecificCategoryModel moneyBackSpecificRuleModel)
    {
        return new MoneyBackCategoryEf
        {
            Id = moneyBackSpecificRuleModel.Id,
            MoneyBackRulesId = moneyBackSpecificRuleModel.MoneyBackRulesId,
            MCC = moneyBackSpecificRuleModel.MCC,
            Name = moneyBackSpecificRuleModel.Name,
            Description = moneyBackSpecificRuleModel.Description,
            Percent = moneyBackSpecificRuleModel.Percent
        };
    }

    public static MoneyBackPartnerEf FromModel(this MoneyBackPartnerModel moneyBackPartnerModel)
    {
        return new MoneyBackPartnerEf
        {
            Id = moneyBackPartnerModel.Id,
            MoneyBackRulesId = moneyBackPartnerModel.MoneyBackRulesId,
            Name = moneyBackPartnerModel.Name,
            Percent = moneyBackPartnerModel.Percent
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
            IsPerpetual = depositOffer.DepositTerm.IsPerpetual,
            DepositTermValue = depositOffer.DepositTerm.Value,
            DepositTermDuration = depositOffer.DepositTerm.Scale,
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

    public static MoneyBackRulesEf ToEf(this MoneyBackRules moneyBackRules)
    {
        return new MoneyBackRulesEf
        {
            Id = moneyBackRules.Id,
            DepositOfferId = moneyBackRules.DepositOfferId,
            StartDate = moneyBackRules.StartDate,
            MoneyBackType = moneyBackRules.MoneyBackType,
            Percent = moneyBackRules.Percent,
            MinPurchaseAmount = moneyBackRules.MinPurchaseAmount,
            MaxPurchaseAmount = moneyBackRules.MaxPurchaseAmount,
            MaxMoneyBackAmount = moneyBackRules.MaxMoneyBackAmount,
            MinMonthPurchasesAmount = moneyBackRules.MinMonthPurchasesAmount,
            Comment = moneyBackRules.Comment
        };
    }

    public static MoneyBackRules FromEf(this MoneyBackRulesEf moneyBackRulesEf)
    {
        return new MoneyBackRules
        {
            Id = moneyBackRulesEf.Id,
            DepositOfferId = moneyBackRulesEf.DepositOfferId,
            StartDate = moneyBackRulesEf.StartDate,
            MoneyBackType = moneyBackRulesEf.MoneyBackType,
            Percent = moneyBackRulesEf.Percent,
            MinPurchaseAmount = moneyBackRulesEf.MinPurchaseAmount,
            MaxPurchaseAmount = moneyBackRulesEf.MaxPurchaseAmount,
            MaxMoneyBackAmount = moneyBackRulesEf.MaxMoneyBackAmount,
            MinMonthPurchasesAmount = moneyBackRulesEf.MinMonthPurchasesAmount,
            Comment = moneyBackRulesEf.Comment
        };
    }

    public static MoneyBackCategoryEf ToEf(this MoneyBackCategory moneyBackCategory)
    {
        return new MoneyBackCategoryEf
        {
            Id = moneyBackCategory.Id,
            MoneyBackRulesId = moneyBackCategory.MoneyBackRulesId,
            MCC = moneyBackCategory.MCC,
            Name = moneyBackCategory.Name,
            Description = moneyBackCategory.Description,
            Percent = moneyBackCategory.Percent
        };
    }

    public static MoneyBackCategory FromEf(this MoneyBackCategoryEf moneyBackCategoryEf)
    {
        return new MoneyBackCategory
        {
            Id = moneyBackCategoryEf.Id,
            MoneyBackRulesId = moneyBackCategoryEf.MoneyBackRulesId,
            MCC = moneyBackCategoryEf.MCC,
            Name = moneyBackCategoryEf.Name,
            Description = moneyBackCategoryEf.Description,
            Percent = moneyBackCategoryEf.Percent
        };
    }

    public static MoneyBackPartnerEf ToEf(this MoneyBackPartner moneyBackPartner)
    {
        return new MoneyBackPartnerEf
        {
            Id = moneyBackPartner.Id,
            MoneyBackRulesId = moneyBackPartner.MoneyBackRulesId,
            Name = moneyBackPartner.Name,
            Percent = moneyBackPartner.Percent
        };
    }

    public static MoneyBackPartner FromEf(this MoneyBackPartnerEf moneyBackPartnerEf)
    {
        return new MoneyBackPartner
        {
            Id = moneyBackPartnerEf.Id,
            MoneyBackRulesId = moneyBackPartnerEf.MoneyBackRulesId,
            Name = moneyBackPartnerEf.Name,
            Percent = moneyBackPartnerEf.Percent
        };
    }
}
