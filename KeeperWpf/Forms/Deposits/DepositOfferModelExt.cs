using System;
using System.Linq;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public static class DepositOfferModelExt
{
    public static decimal GetCurrentRate(this DepositOfferModel depositOfferModel, 
        DateTime openingDate, out string rateFormula)
    {
        var orderedConditions = depositOfferModel.CondsList.OrderBy(c => c.DateFrom).ToList();
        var conditions = orderedConditions.First();
        foreach (var currentConditions in orderedConditions.TakeWhile(c => c.DateFrom <= openingDate))
        {
            conditions = currentConditions;
        }

        rateFormula = depositOfferModel.RateType != RateType.Linked ? "" : conditions.RateFormula;
        return conditions.RateLines.Last().Rate;
    }
}