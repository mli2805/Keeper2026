using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class ButtonCollectionsRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<ButtonCollectionModel>> GetAllButtonCollections(Dictionary<int, AccountItemModel> AcMoDict)
    {
        List<ButtonCollectionEf> bcs = await keeperDbContext.ButtonCollections.ToListAsync();
        var result = new List<ButtonCollectionModel>(bcs.Count);
        foreach (var bc in bcs)
        {
            var ids = bc.AccountIdsString
                .Split(';', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()!;
            var bcm = new ButtonCollectionModel()
            {
                Id = bc.Id,
                Name = bc.Name,
                AccountModels = ids.Select(id => AcMoDict[id]).ToList()
            };

            result.Add(bcm);
        }
        return result;
    }

    public List<ButtonCollection> GetAllButtonCollections()
    {
        var result = keeperDbContext.ButtonCollections.Select(bc => bc.FromEf()).ToList();
        return result;
    }
}
