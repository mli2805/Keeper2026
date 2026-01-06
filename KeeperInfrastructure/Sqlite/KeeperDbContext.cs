using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class KeeperDbContext : DbContext
{
    public DbSet<AccountEf> Accounts { get; set; }
    public DbSet<CarEf> Cars { get; set; }
    public DbSet<CarYearMileageEf> CarYearMileages { get; set; }
    public DbSet<DepositOfferEf> DepositOffers { get; set; }
    public DbSet<DepositConditionsEf> DepositConditions { get; set; }
    public DbSet<DepositRateLineEf> DepositRateLines { get; set; }


    public KeeperDbContext(DbContextOptions<KeeperDbContext> options) : base(options)
    {
    }
}
