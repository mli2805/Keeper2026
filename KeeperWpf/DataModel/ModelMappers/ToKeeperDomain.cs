using KeeperDomain;
using System.Collections.Generic;
using System.Linq;

namespace KeeperWpf;

public static class ToKeeperDomain
{
    public static KeeperDomainModel From(this KeeperDataModel keeperDataModel)
    {
        var accountPlaneList = keeperDataModel.FlattenAccountTree().ToList();
        var bankAccounts = keeperDataModel.AcMoDict.Values
                .Where(a => a.IsBankAccount)
                .Select(ac => ac.BankAccount.FromModel())
                .ToList();
        var deposits = keeperDataModel.AcMoDict.Values
            .Where(a => a.IsDeposit)
            .Select(ac => ac.BankAccount.Deposit!)
            .ToList();
        var payCards = keeperDataModel.AcMoDict.Values
            .Where(a => a.IsCard)
            .Select(ac => ac.BankAccount.PayCard!)
            .ToList();

        var depositOffers = new List<DepositOffer>();
        var depositRateLines = new List<DepositRateLine>();
        var depositConditions = new List<DepositConditions>();
        foreach (var depositOffer in keeperDataModel.DepositOffers)
        {
            foreach (var pair in depositOffer.CondsMap)
            {
                depositRateLines.AddRange(pair.Value.RateLines);
                depositConditions.Add(pair.Value.FromModel());
            }
            depositOffers.Add(depositOffer.FromModel());
        }

        var result = new KeeperDomainModel()
        {
            OfficialRates = keeperDataModel.OfficialRates.Values.ToList(),
            ExchangeRates = keeperDataModel.ExchangeRates.Values.ToList(),
            MetalRates = keeperDataModel.MetalRates,
            RefinancingRates = keeperDataModel.RefinancingRates,

            TrustAssets = keeperDataModel.InvestmentAssets.Select(t => t.FromModel()).ToList(),
            TrustAssetRates = keeperDataModel.AssetRates,
            TrustAccounts = keeperDataModel.TrustAccounts,
            TrustTransactions = keeperDataModel.InvestTranModels.Select(t => t.FromModel()).ToList(),

            AccountPlaneList = accountPlaneList,
            BankAccounts = bankAccounts,
            Deposits = deposits,
            PayCards = payCards,

            DepositOffers = depositOffers,
            DepositRateLines = depositRateLines,
            DepositConditions = depositConditions,

            Transactions = keeperDataModel.Transactions.Values.Select(t => t.FromModel()).ToList(),
            Fuellings = keeperDataModel.FuellingVms.Select(f => f.FromModel()).ToList(),

            Cars = keeperDataModel.Cars.Select(c => c.FromModel()).ToList(),
            CarYearMileages = keeperDataModel.Cars
                        .SelectMany(c => c.YearsMileage)
                        .Select(y => y.FromModel()).ToList(),

            CardBalanceMemos = keeperDataModel.CardBalanceMemoModels.Select(cbm => cbm.FromModel()).ToList(),
            ButtonCollections = keeperDataModel.ButtonCollections.Select(bc => bc.FromModel()).ToList(),
            SalaryChanges = keeperDataModel.SalaryChanges,
            LargeExpenseThresholds = keeperDataModel.LargeExpenseThresholds,
        };
        return result;
    }
}
