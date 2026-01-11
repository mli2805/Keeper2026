using KeeperDomain;

namespace KeeperInfrastructure;

public class DepositOffersRepository(KeeperDbContext keeperDbContext)
{
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


