using System;

namespace KeeperWpf;

public static class InvestmentAssetExt
{
    public static decimal GetAccumulatedCoupon(this TrustAssetModel asset, DateTime date)
    {
        var days = (date - asset.PreviousCouponDate).Days;
        return asset.Nominal * (decimal)asset.CouponRate / 100 * days / 365;
    }
}
