using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace KeeperWpf;

public static class EntitiesToModels
{
    public static AccountItemModel ToModel(this Account account)
    {
        return new AccountItemModel(account.Id, account.Name, null)
        {
            Id = account.Id,
            IsFolder = account.IsFolder,
            IsExpanded = account.IsExpanded,
            AssociatedIncomeId = account.AssociatedIncomeId,
            AssociatedExpenseId = account.AssociatedExpenseId,
            AssociatedExternalId = account.AssociatedExternalId,
            AssociatedTagId = account.AssociatedTagId,
            ShortName = account.ShortName,
            ButtonName = account.ButtonName,
            Comment = account.Comment,
        };
    }

    public static BankAccountModel ToModel(this BankAccount bankAccount)
    {
        return new BankAccountModel
        {
            Id = bankAccount.Id,
            BankId = bankAccount.BankId,
            DepositOfferId = bankAccount.DepositOfferId,
            MainCurrency = bankAccount.MainCurrency,
            AgreementNumber = bankAccount.AgreementNumber,
            ReplenishDetails = bankAccount.ReplenishDetails,
            StartDate = bankAccount.StartDate,
            FinishDate = bankAccount.FinishDate,
            IsMine = bankAccount.IsMine,
        };
    }

    public static DepositOfferModel ToModel(this DepositOffer depositOffer, Dictionary<int, AccountItemModel> acMoDict)
    {
        return new DepositOfferModel
        {
            Id = depositOffer.Id,
            Bank = acMoDict[depositOffer.BankId],
            Title = depositOffer.Title,
            IsNotRevocable = depositOffer.IsNotRevocable,
            RateType = depositOffer.RateType,
            IsAddLimited = depositOffer.IsAddLimited,
            AddLimitInDays = depositOffer.AddLimitInDays,
            MainCurrency = depositOffer.MainCurrency,
            DepositTerm = depositOffer.DepositTerm.ToModel(),
            MonthPaymentsMinimum = depositOffer.MonthPaymentsMinimum,
            MonthPaymentsMaximum = depositOffer.MonthPaymentsMaximum,
            Comment = depositOffer.Comment,
        };
    }

    private static DurationModel ToModel(this Duration duration)
    {
        return duration.IsPerpetual ? new DurationModel() : new DurationModel(duration.Value, duration.Scale);
    }

    public static TransactionModel ToModel(this Transaction transaction, Dictionary<int, AccountItemModel> acMoDict)
    {
        return new TransactionModel()
        {
            Id = transaction.Id,
            Timestamp = transaction.Timestamp,
            Receipt = transaction.Receipt,
            Operation = transaction.Operation,
            PaymentWay = transaction.PaymentWay,
            MyAccount = acMoDict[transaction.MyAccount],
            MySecondAccount = transaction.MySecondAccount == -1 ? null : acMoDict[transaction.MySecondAccount],
            Counterparty = transaction.Counterparty <= 0 ? null : acMoDict[transaction.Counterparty],
            Category = transaction.Category <= 0 ? null : acMoDict[transaction.Category],
            Amount = transaction.Amount,
            AmountInReturn = transaction.AmountInReturn,
            Currency = transaction.Currency,
            CurrencyInReturn = transaction.CurrencyInReturn,
            Tags = transaction.Tags.ToModels(acMoDict),
            Comment = transaction.Comment,
        };
    }

    private static List<AccountItemModel> ToModels(this string tagStr, Dictionary<int, AccountItemModel> acMoDict)
    {
        var tags = new List<AccountItemModel>();
        if (tagStr == "" || tagStr == " ") return tags;

        var substrings = tagStr.Split('|');
        tags.AddRange(substrings
            .Select(substring => int.Parse(substring.Trim()))
            .Select(i => acMoDict[i]));

        return tags;
    }

    public static CardBalanceMemoModel ToModel(this CardBalanceMemo entity, AccountItemModel account)
    {
        return new CardBalanceMemoModel()
        {
            Id = entity.Id,
            Account = account,
            BalanceThreshold = entity.BalanceThreshold,
        };
    }

    public static TrustAssetModel ToModel(this TrustAsset asset, KeeperDataModel dataModel)
    {
        return new TrustAssetModel()
        {
            Id = asset.Id,
            TrustAccount = asset.Id == 0 ? null : dataModel.TrustAccounts.FirstOrDefault(t => t.Id == asset.TrustAccountId),
            Ticker = asset.Ticker,
            Title = asset.Title,
            StockMarket = asset.StockMarket,
            AssetType = asset.AssetType,
            Nominal = asset.Nominal,
            BondCouponPeriod = asset.BondCouponPeriod,
            CouponRate = asset.CouponRate,
            PreviousCouponDate = asset.PreviousCouponDate,
            BondExpirationDate = asset.BondExpirationDate,
            Comment = asset.Comment,
        };
    }

    public static TrustTranModel ToModel(this TrustTransaction transaction, KeeperDataModel dataModel)
    {
        return new TrustTranModel()
        {
            Id = transaction.Id,
            InvestOperationType = transaction.InvestOperationType,
            Timestamp = transaction.Timestamp,
            AccountItemModel = transaction.AccountId != 0 ? dataModel.AcMoDict[transaction.AccountId] : null,
            TrustAccount = dataModel.TrustAccounts.FirstOrDefault(t => t.Id == transaction.TrustAccountId),
            CurrencyAmount = transaction.CurrencyAmount,
            CouponAmount = transaction.CouponAmount,
            Currency = transaction.Currency,
            AssetAmount = transaction.AssetAmount,
            Asset = dataModel.InvestmentAssets.FirstOrDefault(a => a.Id == transaction.AssetId),
            BuySellFee = transaction.PurchaseFee,
            BuySellFeeCurrency = transaction.PurchaseFeeCurrency == 0 ? CurrencyCode.BYN : transaction.PurchaseFeeCurrency,
            FeePaymentOperationId = transaction.FeePaymentOperationId,
            Comment = transaction.Comment,
        };
    }

    public static ButtonCollectionModel ToModel(this ButtonCollection buttonCollection, Dictionary<int, AccountItemModel> acMoDict)
    {
        return new ButtonCollectionModel()
        {
            Id = buttonCollection.Id,
            Name = buttonCollection.Name,
            AccountModels = buttonCollection.AccountIds.Select(i => acMoDict[i]).ToList(),
        };
    }

}