using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class SalaryChangesRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<SalaryChange>> GetAllSalaryChanges()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var result = keeperDbContext.SalaryChanges.Select(sc => sc.FromEf()).ToList();
        return result;
    }

    public async Task SaveAll(List<SalaryChange> lines)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        foreach (SalaryChange salaryChange in lines)
        {
            var salaryChangeEf = await keeperDbContext.SalaryChanges.FirstOrDefaultAsync(s => s.Id == salaryChange.Id);
            if (salaryChangeEf == null)
            {
                salaryChangeEf = salaryChange.ToEf();
                await keeperDbContext.SalaryChanges.AddAsync(salaryChangeEf);
            }
            else
            {
                salaryChangeEf.EmployerId = salaryChange.EmployerId;
                salaryChangeEf.FirstReceived = salaryChange.FirstReceived;
                salaryChangeEf.Amount = salaryChange.Amount;
                salaryChangeEf.Comment = salaryChange.Comment;
            }
        }

        foreach (SalaryChangeEf salaryChangeEf in keeperDbContext.SalaryChanges)
        {
            if (lines.FirstOrDefault(l => l.Id == salaryChangeEf.Id) == null)
            {
                keeperDbContext.SalaryChanges.Remove(salaryChangeEf);
            }
        }

        await keeperDbContext.SaveChangesAsync();
    }
}