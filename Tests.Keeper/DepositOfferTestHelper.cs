using KeeperDomain;
using KeeperModels;

namespace Tests.Keeper;

public static class DepositOfferTestHelper
{
    public static DepositOfferModel CreateDepositOfferModel(Dictionary<int, AccountItemModel> acMoDict)
    {
        var model = new DepositOfferModel
        {
            Bank = acMoDict.Values.First(a => a.Name == "БАПБ"),
            Title = "Тестовый вклад",
            MainCurrency = CurrencyCode.BYN,
            MonthPaymentsMinimum = 100,
            Comment = "Комментарий к тестовому вкладу",
            CondsMap = []
        };

        model.CondsMap.Add(DateTime.Today.AddDays(-30), CreateDepoCondsModel(DateTime.Today.AddDays(-30)));
        model.CondsMap.Add(DateTime.Today, CreateDepoCondsModel(DateTime.Today));
        return model;
    }

    public static DepositOfferModel ChangeDepositOfferModel(DepositOfferModel model)
    {
        model.Title = "Обновленный тестовый вклад";
        model.MonthPaymentsMinimum += 100;

        var conds1 = model.CondsMap[DateTime.Today.AddDays(-30)];
        conds1.IsFactDays = !conds1.IsFactDays;
        var rateLine2 = conds1.RateLines[1];
        rateLine2.Rate += 0.5m;
        conds1.RateLines.Add(CreateDepositRateLine(DateTime.Today.AddDays(-30), 5000, 10000, 5));
        var conds2 = CreateDepoCondsModel(DateTime.Today.AddDays(-15));
        model.CondsMap[DateTime.Today.AddDays(-15)] = conds2;
        model.CondsMap.Remove(DateTime.Today);

        return model;
    }

    private static DepoCondsModel CreateDepoCondsModel(DateTime date)
    {
        return new DepoCondsModel
        {
            DateFrom = date,
            IsFactDays = true,
            RateLines = new List<DepositRateLine>()
            {
                CreateDepositRateLine(date, 0, 1000, 3),
                CreateDepositRateLine(date, 1000, 5000, 4),
            }
        };
    }

    private static DepositRateLine CreateDepositRateLine(DateTime date, decimal amountFrom, decimal amountTo, decimal rate)
    {
        return new DepositRateLine
        {
            DateFrom = date,
            AmountFrom = amountFrom,
            AmountTo = amountTo,
            Rate = rate
        };
    }
}
