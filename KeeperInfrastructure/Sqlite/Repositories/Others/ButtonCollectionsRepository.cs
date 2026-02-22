using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class ButtonCollectionsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<ButtonCollectionModel>> GetAllButtonCollections(Dictionary<int, AccountItemModel> AcMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
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

    public async Task SaveAll(List<ButtonCollectionModel> lines)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        foreach (ButtonCollectionModel buttonCollection in lines)
        {
            var buttonCollectionEf = await keeperDbContext.ButtonCollections
                .FirstOrDefaultAsync(s => s.Id == buttonCollection.Id);
            if (buttonCollectionEf == null)
            {
                buttonCollectionEf = buttonCollection.ToEf();
                await keeperDbContext.ButtonCollections.AddAsync(buttonCollectionEf);
            }
            else
            {
                buttonCollectionEf.Name = buttonCollection.Name;
                buttonCollectionEf.AccountIdsString = string.Join(';', buttonCollection.AccountModels.Select(am => am.Id));
            }
        }
        foreach (ButtonCollectionEf buttonCollectionEf in keeperDbContext.ButtonCollections)
        {
            if (lines.FirstOrDefault(l => l.Id == buttonCollectionEf.Id) == null)
            {
                keeperDbContext.ButtonCollections.Remove(buttonCollectionEf);
            }
        }
        await keeperDbContext.SaveChangesAsync();
    }

}
