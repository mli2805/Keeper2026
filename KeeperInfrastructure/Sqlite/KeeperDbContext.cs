using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class KeeperDbContext : DbContext
{
    public DbSet<AccountEf> Accounts { get; set; }
    public DbSet<BankAccountEf> BankAccounts { get; set; }
    public DbSet<DepositEf> Deposits { get; set; }
    public DbSet<PayCardEf> PayCards { get; set; }

    public DbSet<TrustAccountEf> TrustAccounts { get; set; }
    public DbSet<TrustAssetEf> TrustAssets { get; set; }
    public DbSet<TrustAssetRateEf> TrustAssetRates { get; set; }
    public DbSet<TrustTransactionEf> TrustTransactions { get; set; }

    public DbSet<OfficialRatesEf> OfficialRates { get; set; }
    public DbSet<ExchangeRatesEf> ExchangeRates { get; set; }
    public DbSet<RefinancingRateEf> RefinancingRates { get; set; }
    public DbSet<MetalRateEf> MetalRates { get; set; }

    public DbSet<CarEf> Cars { get; set; }
    public DbSet<CarYearMileageEf> CarYearMileages { get; set; }

    public DbSet<DepositOfferEf> DepositOffers { get; set; }
    public DbSet<DepositConditionsEf> DepositConditions { get; set; }
    public DbSet<DepositRateLineEf> DepositRateLines { get; set; }

    public DbSet<TransactionEf> Transactions { get; set; }
    public DbSet<FuellingEf> Fuellings { get; set; }

    public DbSet<SalaryChangeEf> SalaryChanges { get; set; }
    public DbSet<CardBalanceMemoEf> CardBalanceMemos { get; set; }
    public DbSet<LargeExpenseThresholdEf> LargeExpenseThresholds { get; set; }
    public DbSet<ButtonCollectionEf> ButtonCollections { get; set; }


    public KeeperDbContext(DbContextOptions<KeeperDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarEf>()
            .HasMany(c => c.YearMileages)
            .WithOne(m => m.Car)
            .HasForeignKey(m => m.CarId);

        modelBuilder.Entity<DepositOfferEf>()
            .HasMany(o => o.Conditions)
            .WithOne(c => c.DepositOffer)
            .HasForeignKey(c => c.DepositOfferId);

        modelBuilder.Entity<DepositConditionsEf>()
            .HasMany(c => c.RateLines)
            .WithOne(rl => rl.DepositConditions)
            .HasForeignKey(rl => rl.DepositOfferConditionsId);
    }
}
