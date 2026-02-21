using KeeperModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KeeperInfrastructure;

public class DepositOffersRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<DepositOfferModel>> GetDepositOffersWithConditionsAndRates(Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var offersEf = await keeperDbContext.DepositOffers
            .Include(o => o.Conditions)
            .ThenInclude(c => c.RateLines)
            .ToListAsync();

        return offersEf.Select(o => o.ToModel(acMoDict)).ToList();
    }

    // возвращает добавленную офферу с условиями и ставками с заполненным Id во всех сущностях
    public async Task<DepositOfferModel> AddDepositOffer(DepositOfferModel offerModel, Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        EntityEntry<DepositOfferEf> entityEntry = await keeperDbContext.DepositOffers.AddAsync(offerModel.FromModel());
        await keeperDbContext.SaveChangesAsync();
        return entityEntry.Entity.ToModel(acMoDict);
    }

    public async Task<DepositOfferModel> UpdateDepositOffer(DepositOfferModel offerModel, Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var existingOfferEf = await keeperDbContext.DepositOffers
            .Include(o => o.Conditions)
            .ThenInclude(c => c.RateLines)
            .FirstAsync(o => o.Id == offerModel.Id);


        // Обновляем существующую офферу
        existingOfferEf.UpdateEf(offerModel);

        // Обновляем условия и ставки
        var modelConditions = offerModel.CondsMap.Values.ToList();
        var modelConditionsIds = modelConditions.Where(c => c.Id > 0).Select(c => c.Id).ToList();

        // Удаляем условия, которых нет в модели
        var conditionsToRemove = existingOfferEf.Conditions
            .Where(c => !modelConditionsIds.Contains(c.Id))
            .ToList();
        foreach (var condition in conditionsToRemove)
        {
            keeperDbContext.DepositConditions.Remove(condition);
        }

        // Обновляем или добавляем условия
        foreach (var modelCondition in modelConditions)
        {
            var existingConditionEf = existingOfferEf.Conditions
                .FirstOrDefault(c => c.Id == modelCondition.Id);

            if (existingConditionEf != null)
            {
                // Обновляем существующее условие
                existingConditionEf.UpdateEf(modelCondition);

                // Обновляем ставки для этого условия
                var modelRateLinesIds = modelCondition.RateLines.Where(r => r.Id > 0).Select(r => r.Id).ToList();

                // Удаляем ставки, которых нет в модели
                var rateLinesToRemove = existingConditionEf.RateLines
                    .Where(r => !modelRateLinesIds.Contains(r.Id))
                    .ToList();
                foreach (var rateLine in rateLinesToRemove)
                {
                    keeperDbContext.DepositRateLines.Remove(rateLine);
                }

                // Обновляем или добавляем ставки
                foreach (var modelRateLine in modelCondition.RateLines)
                {
                    var existingRateLineEf = existingConditionEf.RateLines
                        .FirstOrDefault(r => r.Id == modelRateLine.Id);

                    if (existingRateLineEf != null)
                    {
                        // Обновляем существующую ставку
                        existingRateLineEf.UpdateEf(modelRateLine);
                    }
                    else
                    {
                        // Добавляем новую ставку используя маппер
                        var newRateLine = modelRateLine.ToEf();
                        newRateLine.DepositOfferConditionsId = existingConditionEf.Id;
                        existingConditionEf.RateLines.Add(newRateLine);
                    }
                }
            }
            else
            {
                // Добавляем новое условие используя маппер
                var newConditionEf = modelCondition.FromModel();
                newConditionEf.DepositOfferId = existingOfferEf.Id;
                existingOfferEf.Conditions.Add(newConditionEf);
            }
        }

        await keeperDbContext.SaveChangesAsync();
        return existingOfferEf.ToModel(acMoDict);
    }

    public async Task DeleteDepositOffer(int offerId)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var offerEf = await keeperDbContext.DepositOffers
            .Include(o => o.Conditions)
            .ThenInclude(c => c.RateLines)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offerEf != null)
        {
            keeperDbContext.DepositOffers.Remove(offerEf);
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
