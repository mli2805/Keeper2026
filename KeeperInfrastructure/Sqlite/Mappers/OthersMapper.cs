using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace KeeperInfrastructure;

public static class OthersMapper
{
    public static LargeExpenseThresholdEf ToEf(this LargeExpenseThreshold domain)
    {
        return new LargeExpenseThresholdEf
        {
            Id = domain.Id,
            FromDate = domain.FromDate,
            Amount = domain.Amount,
            AmountForYearAnalysis = domain.AmountForYearAnalysis
        };
    }

    public static LargeExpenseThreshold FromEf(this LargeExpenseThresholdEf ef)
    {
        return new LargeExpenseThreshold
        {
            Id = ef.Id,
            FromDate = ef.FromDate,
            Amount = ef.Amount,
            AmountForYearAnalysis = ef.AmountForYearAnalysis
        };
    }

    public static CardBalanceMemoEf ToEf(this CardBalanceMemo domain)
    {
        return new CardBalanceMemoEf
        {
            Id = domain.Id,
            AccountId = domain.AccountId,
            BalanceThreshold = domain.BalanceThreshold
        };
    }

    public static CardBalanceMemo FromEf(this CardBalanceMemoEf ef)
    {
        return new CardBalanceMemo
        {
            Id = ef.Id,
            AccountId = ef.AccountId,
            BalanceThreshold = ef.BalanceThreshold
        };
    }

    public static SalaryChangeEf ToEf(this SalaryChange domain)
    {
        return new SalaryChangeEf
        {
            Id = domain.Id,
            EmployerId = domain.EmployerId,
            FirstReceived = domain.FirstReceived,
            Amount = domain.Amount,
            Comment = domain.Comment
        };
    }

    public static SalaryChange FromEf(this SalaryChangeEf ef)
    {
        return new SalaryChange
        {
            Id = ef.Id,
            EmployerId = ef.EmployerId,
            FirstReceived = ef.FirstReceived,
            Amount = ef.Amount,
            Comment = ef.Comment
        };
    }
}
