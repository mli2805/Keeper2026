using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace KeeperWpf;

public static class EntitiesJoiner
{
    public static List<CarModel> JoinCarParts(List<Car> cars, List<CarYearMileage> carYearMileages)
    {
        var result = cars.Select(c => Mapper.Map<CarModel>(c)).ToList();
        foreach (var car in result)
        {
            var arr = carYearMileages.Where(l => l.CarId == car.Id).ToArray();
            // var prev = car.PurchaseMileage;
            foreach (var y in arr)
            {
                var cc = Mapper.Map<YearMileageModel>(y);
                // cc.Mileage = y.Odometer - prev;
                car.YearsMileage.Add(cc);
                // prev = y.Odometer;
            }
        }
        return result;
    }

    public static List<DepositOfferModel> JoinDepoParts(this KeeperBin bin, Dictionary<int, AccountItemModel> acMoDict)
    {
        var result = bin.DepositOffers.Select(o => o.Map(acMoDict)).ToList();
        foreach (var depoOffer in result)
        {
            foreach (var depoCondition in bin.DepositConditions.Where(c => c.DepositOfferId == depoOffer.Id))
            {
                var depoCondsModel = Mapper.Map<DepoCondsModel>(depoCondition);

                depoCondsModel.RateLines = bin.DepositRateLines
                    .Where(l => l.DepositOfferConditionsId == depoCondition.Id)
                    .OrderBy(r => r.DateFrom)
                    .ThenBy(r => r.AmountFrom).ToList();

                depoOffer.CondsMap.Add(depoCondition.DateFrom, depoCondsModel);
            }
        }
        return result;
    }
}
