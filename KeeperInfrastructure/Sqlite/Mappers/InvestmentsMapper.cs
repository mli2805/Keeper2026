using KeeperDomain;

namespace KeeperInfrastructure;

public static class InvestmentsMapper
{
    public static TrustAccountEf ToEf(this TrustAccount trustAccount)
    {
        return new TrustAccountEf
        {
            Id = trustAccount.Id,
            Title = trustAccount.Title,
            StockMarket = trustAccount.StockMarket,
            Number = trustAccount.Number,
            Currency = trustAccount.Currency,
            AccountId = trustAccount.AccountId,
            Comment = trustAccount.Comment
        };
    }

    public static TrustAccount FromEf(this TrustAccountEf trustAccountEf)
    {
        return new TrustAccount
        {
            Id = trustAccountEf.Id,
            Title = trustAccountEf.Title,
            StockMarket = trustAccountEf.StockMarket,
            Number = trustAccountEf.Number,
            Currency = trustAccountEf.Currency,
            AccountId = trustAccountEf.AccountId,
            Comment = trustAccountEf.Comment
        };
    }

    public static TrustAssetEf ToEf(this TrustAsset trustAsset)
    {
        return new TrustAssetEf
        {
            Id = trustAsset.Id,
            TrustAccountId = trustAsset.TrustAccountId,
            Ticker = trustAsset.Ticker,
            Title = trustAsset.Title,
            StockMarket = trustAsset.StockMarket,
            AssetType = trustAsset.AssetType,
            Nominal = trustAsset.AssetType == AssetType.Bond ? trustAsset.Nominal : null,
            BondCouponDurationValue = trustAsset.AssetType == AssetType.Bond ? trustAsset.BondCouponPeriod.Value : null,
            BondCouponDuration = trustAsset.AssetType == AssetType.Bond ? trustAsset.BondCouponPeriod.Scale : null,
            CouponRate = trustAsset.AssetType == AssetType.Bond ? trustAsset.CouponRate : null,
            PreviousCouponDate = trustAsset.AssetType == AssetType.Bond ? trustAsset.PreviousCouponDate : null,
            BondExpirationDate = trustAsset.AssetType == AssetType.Bond ? trustAsset.BondExpirationDate : null,
            Comment = trustAsset.Comment
        };
    }

    public static TrustAsset FromEf(this TrustAssetEf trustAssetEf)
    {
        return new TrustAsset
        {
            Id = trustAssetEf.Id,
            TrustAccountId = trustAssetEf.TrustAccountId,
            Ticker = trustAssetEf.Ticker,
            Title = trustAssetEf.Title,
            StockMarket = trustAssetEf.StockMarket,
            AssetType = trustAssetEf.AssetType,

            Nominal = trustAssetEf.Nominal ?? 0,
            BondCouponPeriod = trustAssetEf.AssetType == AssetType.Bond 
                ? new Duration(trustAssetEf.BondCouponDurationValue!.Value, trustAssetEf.BondCouponDuration!.Value) 
                : new Duration(),
            CouponRate = trustAssetEf.CouponRate ?? 0,
            PreviousCouponDate = trustAssetEf.PreviousCouponDate ?? DateTime.MinValue,
            BondExpirationDate = trustAssetEf.BondExpirationDate ?? DateTime.MaxValue,
            Comment = trustAssetEf.Comment
        };
    }

    public static TrustAssetRateEf ToEf(this TrustAssetRate trustAssetRate)
    {
        return new TrustAssetRateEf
        {
            TrustAssetId = trustAssetRate.TrustAssetId,
            Date = trustAssetRate.Date,
            Value = trustAssetRate.Value,
            Unit = trustAssetRate.Unit,
            Currency = trustAssetRate.Currency
        };
    }

    public static TrustAssetRate FromEf(this TrustAssetRateEf trustAssetRateEf)
    {
        return new TrustAssetRate
        {
            Id = trustAssetRateEf.Id,
            TrustAssetId = trustAssetRateEf.TrustAssetId,
            Date = trustAssetRateEf.Date,
            Value = trustAssetRateEf.Value,
            Unit = trustAssetRateEf.Unit,
            Currency = trustAssetRateEf.Currency
        };
    }

    public static TrustTransactionEf ToEf(this TrustTransaction trustTransaction)
    {
        return new TrustTransactionEf
        {
            Id = trustTransaction.Id,
            InvestOperationType = trustTransaction.InvestOperationType,
            Timestamp = trustTransaction.Timestamp,
            AccountId = trustTransaction.AccountId,
            TrustAccountId = trustTransaction.TrustAccountId,
            CurrencyAmount = trustTransaction.CurrencyAmount,
            CouponAmount = trustTransaction.CouponAmount,
            Currency = trustTransaction.Currency,
            AssetAmount = trustTransaction.AssetAmount,
            AssetId = trustTransaction.AssetId,
            PurchaseFee = trustTransaction.PurchaseFee,
            PurchaseFeeCurrency = trustTransaction.PurchaseFeeCurrency,
            FeePaymentOperationId = trustTransaction.FeePaymentOperationId,
            Comment = trustTransaction.Comment
        };
    }

    public static TrustTransaction FromEf(this TrustTransactionEf trustTransactionEf)
    {
        return new TrustTransaction
        {
            Id = trustTransactionEf.Id,
            InvestOperationType = trustTransactionEf.InvestOperationType,
            Timestamp = trustTransactionEf.Timestamp,
            AccountId = trustTransactionEf.AccountId,
            TrustAccountId = trustTransactionEf.TrustAccountId,
            CurrencyAmount = trustTransactionEf.CurrencyAmount,
            CouponAmount = trustTransactionEf.CouponAmount,
            Currency = trustTransactionEf.Currency,
            AssetAmount = trustTransactionEf.AssetAmount,
            AssetId = trustTransactionEf.AssetId,
            PurchaseFee = trustTransactionEf.PurchaseFee,
            PurchaseFeeCurrency = trustTransactionEf.PurchaseFeeCurrency,
            FeePaymentOperationId = trustTransactionEf.FeePaymentOperationId,
            Comment = trustTransactionEf.Comment
        };
    }
}
