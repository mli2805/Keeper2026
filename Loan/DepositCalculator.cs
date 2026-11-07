using System;

namespace Loan;

public static class DepositCalculator
{
    public static decimal Calculate(decimal amount, DateTime openDate, int years, decimal rate)
    {
        var endDate = openDate.AddYears(years);
        var days = (endDate - openDate).Days;
        return amount * rate / 100m * days / 365m;
    }
}
