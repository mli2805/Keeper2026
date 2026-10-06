using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class SalaryPaymentsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<SalaryPayment>> GetAllSalaryPayments()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var result = keeperDbContext.SalaryPayments.Select(sp => sp.FromEf()).ToList();
        return result;
    }

    public async Task SaveAll(List<SalaryPayment> lines)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        foreach (SalaryPayment salaryPayment in lines)
        {
            var salaryPaymentEf = await keeperDbContext.SalaryPayments
                .FirstOrDefaultAsync(s => s.Id == salaryPayment.Id);
            if (salaryPaymentEf == null)
            {
                salaryPaymentEf = salaryPayment.ToEf();
                await keeperDbContext.SalaryPayments.AddAsync(salaryPaymentEf);
            }
            else
            {
                salaryPaymentEf.EmployerId = salaryPayment.EmployerId;
                salaryPaymentEf.Day = salaryPayment.Day;
                salaryPaymentEf.Category = salaryPayment.Category;
                salaryPaymentEf.Amount = salaryPayment.Amount;
            }
        }

        foreach (SalaryPaymentEf salaryPaymentEf in keeperDbContext.SalaryPayments)
        {
            if (lines.FirstOrDefault(l => l.Id == salaryPaymentEf.Id) == null)
            {
                keeperDbContext.SalaryPayments.Remove(salaryPaymentEf);
            }
        }

        await keeperDbContext.SaveChangesAsync();
    }
}
