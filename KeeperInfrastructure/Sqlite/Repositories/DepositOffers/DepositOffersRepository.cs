using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class DepositOffersRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<DepositOfferModel>> GetDepositOffersWithConditionsAndRates(Dictionary<int, AccountItemModel> acMoDict)
    {
        var offersEf = await keeperDbContext.DepositOffers
            .Include(o => o.Conditions)
                .ThenInclude(c => c.RateLines)
            .ToListAsync();

        var result = new List<DepositOfferModel>();

        foreach (var offerEf in offersEf)
        {
            var offerModel = new DepositOfferModel
            {
                Id = offerEf.Id,
                Bank = acMoDict[offerEf.BankId],
                Title = offerEf.Title,
                IsNotRevocable = offerEf.IsNotRevocable,
                RateType = offerEf.RateType,
                IsAddLimited = offerEf.IsAddLimited,
                AddLimitInDays = offerEf.AddLimitInDays,
                MainCurrency = offerEf.MainCurrency,
                DepositTerm = offerEf.IsPerpetual
                    ? new DurationModel()
                    : new DurationModel(offerEf.DepositTermValue, offerEf.DepositTermDuration),
                MonthPaymentsMinimum = offerEf.MonthPaymentsMinimum,
                MonthPaymentsMaximum = offerEf.MonthPaymentsMaximum,
                Comment = offerEf.Comment
            };

            foreach (var conditionEf in offerEf.Conditions)
            {
                var conditionModel = new DepoCondsModel
                {
                    Id = conditionEf.Id,
                    DepositOfferId = conditionEf.DepositOfferId,
                    DateFrom = conditionEf.DateFrom,
                    RateFormula = conditionEf.RateFormula,
                    IsFactDays = conditionEf.IsFactDays,
                    EveryStartDay = conditionEf.EveryStartDay,
                    EveryFirstDayOfMonth = conditionEf.EveryFirstDayOfMonth,
                    EveryLastDayOfMonth = conditionEf.EveryLastDayOfMonth,
                    EveryNDays = conditionEf.EveryNDays,
                    NDays = conditionEf.NDays,
                    IsCapitalized = conditionEf.IsCapitalized,
                    HasAdditionalPercent = conditionEf.HasAdditionalPercent,
                    AdditionalPercent = conditionEf.AdditionalPercent,
                    Comment = conditionEf.Comment,
                    RateLines = conditionEf.RateLines.Select(rl => new DepositRateLine
                    {
                        Id = rl.Id,
                        DepositOfferConditionsId = rl.DepositOfferConditionsId,
                        DateFrom = rl.DateFrom,
                        AmountFrom = rl.AmountFrom,
                        AmountTo = rl.AmountTo,
                        Rate = rl.Rate
                    }).ToList()
                };

                offerModel.CondsMap.Add(conditionModel.DateFrom, conditionModel);
            }

            result.Add(offerModel);
        }

        return result;
    }

    public List<DepositOffer> GetAllDepositOffers()
    {
        var result = keeperDbContext.DepositOffers.Select(o=>o.FromEf()).ToList();
        return result;
    }

    public List<DepositConditions> GetDepositConditionsByOfferId(int offerId)
    {
        var result = keeperDbContext.DepositConditions
            .Where(c => c.DepositOfferId == offerId)
            .Select(c => c.FromEf())
            .ToList();
        return result;
    }

    public List<DepositRateLine> GetDepositRateLinesByConditionId(int depositConditionsId)
    {
        var result = keeperDbContext.DepositRateLines
            .Where(rl => rl.DepositOfferConditionsId == depositConditionsId)
            .Select(rl => rl.FromEf())
            .ToList();
        return result;
    }
}


