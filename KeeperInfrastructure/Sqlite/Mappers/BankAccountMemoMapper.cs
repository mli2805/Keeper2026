using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class BankAccountMemoMapper
{
    public static BankAccountMemoEf ToEf(this BankAccountMemo domain)
    {
        return new BankAccountMemoEf
        {
            Id = domain.Id,
            AccountId = domain.AccountId,
            BalanceLess = domain.BalanceLess,
            BalanceMore = domain.BalanceMore,
            PaymentsLess = domain.PaymentsLess,
            PaymentsMore = domain.PaymentsMore,
            Comment = domain.Comment
        };
    }

    public static BankAccountMemo FromEf(this BankAccountMemoEf ef)
    {
        return new BankAccountMemo
        {
            Id = ef.Id,
            AccountId = ef.AccountId,
            BalanceLess = ef.BalanceLess,
            BalanceMore = ef.BalanceMore,
            PaymentsLess = ef.PaymentsLess,
            PaymentsMore = ef.PaymentsMore,
            Comment = ef.Comment
        };
    }

    public static BankAccountMemoModel ToModel(this BankAccountMemoEf ef, Dictionary<int, AccountItemModel> acMoDict)
    {
        return new BankAccountMemoModel()
        {
            Id = ef.Id,
            Account = acMoDict[ef.AccountId],
            BalanceLess = new LimitModel { IsOn = ef.BalanceLess.HasValue, LimitValue = ef.BalanceLess ?? 0 },
            BalanceMore = new LimitModel { IsOn = ef.BalanceMore.HasValue, LimitValue = ef.BalanceMore ?? 0 },
            PaymentsLess = new LimitModel { IsOn = ef.PaymentsLess.HasValue, LimitValue = ef.PaymentsLess ?? 0 },
            PaymentsMore = new LimitModel { IsOn = ef.PaymentsMore.HasValue, LimitValue = ef.PaymentsMore ?? 0 },
            Comment = ef.Comment
        };
    }

    public static BankAccountMemo FromModel(this BankAccountMemoModel model)
    {
        return new BankAccountMemo
        {
            Id = model.Id,
            AccountId = model.Account.Id,
            BalanceLess = model.BalanceLess.IsOn ? model.BalanceLess.LimitValue : null,
            BalanceMore = model.BalanceMore.IsOn ? model.BalanceMore.LimitValue : null,
            PaymentsLess = model.PaymentsLess.IsOn ? model.PaymentsLess.LimitValue : null,
            PaymentsMore = model.PaymentsMore.IsOn ? model.PaymentsMore.LimitValue : null,
            Comment = model.Comment
        };
    }

    public static BankAccountMemoEf ToEf(this BankAccountMemoModel model)
    {
        return new BankAccountMemoEf()
        {
            Id = model.Id,
            AccountId = model.Account.Id,
            BalanceLess = model.BalanceLess.IsOn ? model.BalanceLess.LimitValue : null,
            BalanceMore = model.BalanceMore.IsOn ? model.BalanceMore.LimitValue : null,
            PaymentsLess = model.PaymentsLess.IsOn ? model.PaymentsLess.LimitValue : null,
            PaymentsMore = model.PaymentsMore.IsOn ? model.PaymentsMore.LimitValue : null,
            Comment = model.Comment
        };
    }
}