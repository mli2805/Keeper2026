using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class LargeExpenseThresholdsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<LargeExpenseThreshold> GetAllLargeExpenseThresholds()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.LargeExpenseThresholds.Select(let => let.FromEf()).ToList();
        return result;
    }

    public async Task SaveAll(List<LargeExpenseThreshold> lines)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        foreach (LargeExpenseThreshold largeExpenseThreshold in lines)
        {
            var largeExpenseThresholdEf = await keeperDbContext.LargeExpenseThresholds
                .FirstOrDefaultAsync(s => s.Id == largeExpenseThreshold.Id);
            if (largeExpenseThresholdEf == null)
            {
                largeExpenseThresholdEf = largeExpenseThreshold.ToEf();
                await keeperDbContext.LargeExpenseThresholds.AddAsync(largeExpenseThresholdEf);
            }
            else
            {
                largeExpenseThresholdEf.FromDate = largeExpenseThreshold.FromDate;
                largeExpenseThresholdEf.Amount = largeExpenseThreshold.Amount;
                largeExpenseThresholdEf.AmountForYearAnalysis = largeExpenseThreshold.AmountForYearAnalysis;
            }
        }
        foreach (LargeExpenseThresholdEf largeExpenseThresholdEf in keeperDbContext.LargeExpenseThresholds)
        {
            if (lines.FirstOrDefault(l => l.Id == largeExpenseThresholdEf.Id) == null)
            {
                keeperDbContext.LargeExpenseThresholds.Remove(largeExpenseThresholdEf);
            }
        }
        await keeperDbContext.SaveChangesAsync();
    }
}
