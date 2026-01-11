using KeeperDomain;

namespace KeeperInfrastructure;

public class TransactionsRepository(KeeperDbContext keeperDbContext)
{
    public List<Transaction> GetAllTransactions()
    {
        var result = keeperDbContext.Transactions.Select(t => t.FromEf()).ToList();
        return result;

    }
}

public class FuellingsRepository(KeeperDbContext keeperDbContext)
{
    public List<Fuelling> GetAllFuellings()
    {
        var result = keeperDbContext.Fuellings.Select(f => f.FromEf()).ToList();
        return result;
    }
}
