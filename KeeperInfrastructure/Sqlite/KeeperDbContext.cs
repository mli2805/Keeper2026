using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class KeeperDbContext : DbContext
{
    public DbSet<AccountEf> Accounts { get; set; }
    public DbSet<CarEf> Cars { get; set; }
    public DbSet<CarYearMileageEf> CarYearMileages { get; set; }


    public KeeperDbContext(DbContextOptions<KeeperDbContext> options) : base(options)
    {
    }
}
