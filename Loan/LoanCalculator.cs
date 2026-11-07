using System;

namespace Loan;

public static class LoanCalculator
{
    public static (decimal BodyPaid, decimal InterestPaid) Calculate(
        decimal amount, DateTime openDate, int termYears, int graceYears, decimal graceRate)
    {
        var totalMonths = termYears * 12;
        var graceMonths = graceYears * 12;
        var monthlyBody = amount / totalMonths;
        var remaining = amount;
        var bodyPaid = 0m;
        var interestPaid = 0m;
        var periodStart = openDate;

        for (var month = 1; month <= graceMonths; month++)
        {
            var next = openDate.AddMonths(month);
            var interestDate = new DateTime(next.Year, next.Month, 1);
            var days = (interestDate - periodStart).Days;

            interestPaid += remaining * graceRate / 100m * days / 365m;
            bodyPaid += monthlyBody;
            remaining -= monthlyBody;
            periodStart = interestDate;
        }

        return (bodyPaid, interestPaid);
    }
}
