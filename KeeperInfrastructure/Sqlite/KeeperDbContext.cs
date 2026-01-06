using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class KeeperDbContext : DbContext
{
    public DbSet<AccountEf> Accounts { get; set; }
    public DbSet<BankAccountEf> BankAccounts { get; set; }
    public DbSet<DepositEf> Deposits { get; set; }
    public DbSet<PayCardEf> PayCards { get; set; }

    public DbSet<OfficialRatesEf> OfficialRates { get; set; }
    public DbSet<ExchangeRatesEf> ExchangeRates { get; set; }
    public DbSet<RefinancingRateEf> RefinancingRates { get; set; }
    public DbSet<MetalRateEf> MetalRates { get; set; }

    public DbSet<CarEf> Cars { get; set; }
    public DbSet<CarYearMileageEf> CarYearMileages { get; set; }

    public DbSet<DepositOfferEf> DepositOffers { get; set; }
    public DbSet<DepositConditionsEf> DepositConditions { get; set; }
    public DbSet<DepositRateLineEf> DepositRateLines { get; set; }


    public KeeperDbContext(DbContextOptions<KeeperDbContext> options) : base(options)
    {
    }
}
