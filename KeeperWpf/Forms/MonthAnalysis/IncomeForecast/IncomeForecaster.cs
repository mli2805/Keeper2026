using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public class ForeseenIncome
{
    public DateTime ExpectedAt;
    public decimal AmountUsd;
    public string Title = null!;
}

public static class IncomeForecaster
{
    public static List<ForeseenIncome> ForecastIncome2(
        this KeeperDataModel dataModel, DateTime fromDate, DateTime finishMoment)
    {
        var list = dataModel.ForeseeSalary(fromDate, finishMoment).ToList();

        var depoMainFolder = dataModel.AcMoDict[166];
        foreach (var depo in depoMainFolder.Children.Where(c => ((AccountItemModel)c).IsDeposit))
        {
            var depoForecast = dataModel.ForeseeDepoIncome2((AccountItemModel)depo);
            list.AddRange(depoForecast);
        }

        return list;
    }

    private static IEnumerable<ForeseenIncome> ForeseeSalary(
        this KeeperDataModel dataModel, DateTime firstOfMonth, DateTime finishMoment)
    {
        var receivedIncome = dataModel.Transactions.Values
            .Where(t => t.Operation == OperationType.Доход
                        && t.Timestamp >= firstOfMonth && t.Timestamp <= finishMoment).ToList();

        foreach (var payment in dataModel.SalaryPayments)
        {
            if (receivedIncome.Any(t => t.Category!.Id == payment.Category
                                        && t.Counterparty!.Id == payment.EmployerId))
            {
                continue;
            }

            var paymentDate = firstOfMonth.AddDays(payment.Day - 1);
            var category = dataModel.AcMoDict[payment.Category];
            var employer = dataModel.AcMoDict[payment.EmployerId];

            yield return new ForeseenIncome
            {
                ExpectedAt = paymentDate,
                AmountUsd = payment.Amount,
                Title = $"{paymentDate:dd MMM} {category.Name} {employer.Name} {payment.Amount} usd"
            };
        }
    }

    private static IEnumerable<ForeseenIncome> ForeseeDepoIncome2(this KeeperDataModel dataModel, AccountItemModel depo)
    {
        var depoMainCurrency = dataModel.DepositOffers
            .First(o => o.Id == depo.BankAccount!.DepositOfferId).MainCurrency;
        var currency = depoMainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : depoMainCurrency;

        var revenues = depo.GetRevenuesInThisMonth(dataModel);

        return revenues.Select(tuple => new ForeseenIncome
        {
            ExpectedAt = tuple.Item1,
            AmountUsd = currency == CurrencyCode.USD
                ? tuple.Item2
                : dataModel.AmountInUsd(DateTime.Today, depoMainCurrency, tuple.Item2),
            Title = $"{tuple.Item1:dd MMM} {depo.ShortName}  {tuple.Item2:#,0.00} {currency.ToString().ToLower()} "
        });
    }
}
