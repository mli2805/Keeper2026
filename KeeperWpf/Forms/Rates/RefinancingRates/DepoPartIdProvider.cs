using System.Linq;

namespace KeeperWpf;

public static class DepoPartIdProvider
{
    public static int GetDepoConditionsMaxId(this KeeperDataModel dataModel)
    {
        return dataModel.DepositOffers
            .SelectMany(depositOffer => depositOffer.CondsList)
            .ToList()
            .Max(c => c.Id);
    }

    public static int GetDepoRateLinesMaxId(this KeeperDataModel dataModel)
    {
        return dataModel.DepositOffers
            .SelectMany(depositOffer => depositOffer.CondsList)
            .SelectMany(dc=>dc.RateLines)
            .ToList()
            .Max(c => c.Id);
    }
}