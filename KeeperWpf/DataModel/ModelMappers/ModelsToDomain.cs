using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using MigraDoc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace KeeperWpf;

public static class ModelsToDomain
{
    public static Transaction FromModel(this TransactionModel transactionModel)
    {
        return new Transaction()
        {
            Id = transactionModel.Id,
            Timestamp = transactionModel.Timestamp,
            Receipt = transactionModel.Receipt,
            Operation = transactionModel.Operation,
            PaymentWay = transactionModel.PaymentWay,
            MyAccount = transactionModel.MyAccount.Id,
            MySecondAccount = transactionModel.MySecondAccount?.Id ?? -1,
            Counterparty = transactionModel.Counterparty?.Id ?? -1,
            Category = transactionModel.Category?.Id ?? -1,
            Amount = transactionModel.Amount,
            AmountInReturn = transactionModel.AmountInReturn,
            Currency = transactionModel.Currency,
            CurrencyInReturn = transactionModel.CurrencyInReturn,
            Tags = transactionModel.Tags.MapTags(),
            Comment = transactionModel.Comment,
        };
    }

    private static string MapTags(this List<AccountItemModel> tags)
    {
        if (tags == null || tags.Count == 0) return " ";
        string result = "";
        foreach (var t in tags)
        {
            result = result + t.Id + " | ";
        }
        result = result.Substring(0, result.Length - 3);
        return result;
    }

    public static Account FromModel(this AccountItemModel model)
    {
        return new Account()
        {
            Id = model.Id,
            ParentId = model.Parent?.Id ?? 0,
            ChildNumber = model.ChildNumber,
            Name = model.Name,
            IsFolder = model.IsFolder,
            IsExpanded = model.IsExpanded,
            AssociatedIncomeId = model.AssociatedIncomeId,
            AssociatedExpenseId = model.AssociatedExpenseId,
            AssociatedExternalId = model.AssociatedExternalId,
            AssociatedTagId = model.AssociatedTagId,
            ShortName = model.ShortName,
            ButtonName = model.ButtonName,
            Comment = model.Comment,
        };
    }

    public static BankAccount FromModel(this BankAccountModel bankAccountModel)
    {
        return new BankAccount()
        {
            Id = bankAccountModel.Id,
            BankId = bankAccountModel.BankId,
            MainCurrency = bankAccountModel.MainCurrency,
            DepositOfferId = bankAccountModel.DepositOfferId,   
            AgreementNumber = bankAccountModel.AgreementNumber,
            ReplenishDetails = bankAccountModel.ReplenishDetails,
            StartDate = bankAccountModel.StartDate,
            FinishDate = bankAccountModel.FinishDate,
            IsMine = bankAccountModel.IsMine,
        };
    }

    public static DepositOffer FromModel(this DepositOfferModel depositOfferModel)
    {
        return new DepositOffer()
        {
            Id = depositOfferModel.Id,
            BankId = depositOfferModel.Bank.Id,
            Title = depositOfferModel.Title,
            IsNotRevocable = depositOfferModel.IsNotRevocable,
            RateType = depositOfferModel.RateType,
            IsAddLimited = depositOfferModel.IsAddLimited,
            AddLimitInDays = depositOfferModel.AddLimitInDays,
            MainCurrency = depositOfferModel.MainCurrency,
            DepositTerm = depositOfferModel.DepositTerm.FromModel(),
            MonthPaymentsMinimum = depositOfferModel.MonthPaymentsMinimum,
            MonthPaymentsMaximum = depositOfferModel.MonthPaymentsMaximum,
            Comment = depositOfferModel.Comment,
        };
    }

    public static DepositConditions FromModel(this DepoCondsModel depoCondsModel)
    {
        return new DepositConditions()
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
            Comment = depoCondsModel.Comment,
        };
    }

    private static Duration FromModel(this DurationModel durationModel)
    {
        return durationModel.IsPerpetual ? new Duration() : new Duration(durationModel.Value, durationModel.Scale);
    }

   
    public static Fuelling FromModel(this FuellingModel fuellingModel)
    {
        return new Fuelling()
        {
            Id = fuellingModel.Id,
            TransactionId = fuellingModel.Transaction.Id,
            CarAccountId = fuellingModel.CarAccountId,
            Volume = fuellingModel.Volume,
            FuelType = fuellingModel.FuelType,
        };
    }

    public static CardBalanceMemo FromModel(this CardBalanceMemoModel cardBalanceMemoModel)
    {
        return new CardBalanceMemo()
        {
            Id = cardBalanceMemoModel.Id,
            AccountId = cardBalanceMemoModel.Account.Id,
            BalanceThreshold = cardBalanceMemoModel.BalanceThreshold,
        };
    }

    public static TrustAsset FromModel(this TrustAssetModel asset)
    {
        return new TrustAsset()
        {
            Id = asset.Id,
            TrustAccountId = asset.TrustAccount?.Id ?? 0,
            Ticker = asset.Ticker,
            Title = asset.Title,
            StockMarket = asset.StockMarket,
            AssetType = asset.AssetType,
            Nominal = asset.Nominal,
            BondCouponPeriod = asset.BondCouponPeriod.FromModel(),
            CouponRate = asset.CouponRate,
            PreviousCouponDate = asset.PreviousCouponDate,
            BondExpirationDate = asset.BondExpirationDate,
            Comment = asset.Comment,
        };
    }

    public static TrustTransaction FromModel(this TrustTranModel transaction)
    {
        return new TrustTransaction()
        {
            Id = transaction.Id,
            InvestOperationType = transaction.InvestOperationType,
            Timestamp = transaction.Timestamp,
            AccountId = transaction.AccountItemModel?.Id ?? 0,
            TrustAccountId = transaction.TrustAccount?.Id ?? 0,
            CurrencyAmount = transaction.CurrencyAmount,
            CouponAmount = transaction.CouponAmount,
            Currency = transaction.Currency,
            AssetAmount = transaction.AssetAmount,
            AssetId = transaction.Asset?.Id ?? 0,
            PurchaseFee = transaction.BuySellFee,
            PurchaseFeeCurrency = transaction.BuySellFeeCurrency,
            FeePaymentOperationId = transaction.FeePaymentOperationId,
            Comment = transaction.Comment,
        };
    }

    public static ButtonCollection FromModel(this ButtonCollectionModel model)
    {
        return new ButtonCollection()
        {
            Id = model.Id,
            Name = model.Name,
            AccountIds = model.AccountModels.Select(m => m.Id).ToList(),
        };
    }

    public static Car FromModel(this CarModel model)
    {
        return new Car()
        {
            Id = model.Id,
            CarAccountId = model.CarAccountId,
            Title = model.Title,
            IssueYear = model.IssueYear,
            Vin = model.Vin,
            StateRegNumber = model.StateRegNumber,
            PurchaseDate = model.PurchaseDate,
            PurchaseMileage = model.PurchaseMileage,
            SaleDate = model.SaleDate,
            SaleMileage = model.SaleMileage,
            SupposedSalePrice = model.SupposedSalePrice,
            Comment = model.Comment,
        };
    }

    public static CarYearMileage FromModel(this YearMileageModel model)
    {
        return new CarYearMileage()
        {
            Id = model.Id,
            CarId = model.CarId,
            Odometer = model.Odometer,
        };
    }

}