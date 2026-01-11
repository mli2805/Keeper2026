using KeeperDomain;

namespace KeeperInfrastructure;

public class SalaryChangesRepository(KeeperDbContext keeperDbContext)
{
    public List<SalaryChange> GetAllSalaryChanges()
    {
        var result = keeperDbContext.SalaryChanges.Select(sc => sc.FromEf()).ToList();
        return result;
    }
}

public class CardBalanceMemosRepository(KeeperDbContext keeperDbContext)
{
    public List<CardBalanceMemo> GetAllCardBalanceMemos()
    {
        var result = keeperDbContext.CardBalanceMemos.Select(cm => cm.FromEf()).ToList();
        return result;
    }
}

public class LargeExpenseThresholdsRepository(KeeperDbContext keeperDbContext)
{
    public List<LargeExpenseThreshold> GetAllLargeExpenseThresholds()
    {
        var result = keeperDbContext.LargeExpenseThresholds.Select(let => let.FromEf()).ToList();
        return result;
    }
}

public class ButtonCollectionsRepository(KeeperDbContext keeperDbContext)
{
    public List<ButtonCollection> GetAllButtonCollections()
    {
        var result = keeperDbContext.ButtonCollections.Select(bc => bc.FromEf()).ToList();
        return result;
    }
}