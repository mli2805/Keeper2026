using KeeperModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KeeperInfrastructure;

[ExportRepository]
public class DepositOffersRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<DepositOfferModel>> GetDepositOffersWithConditionsAndRates(Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var offersEf = await keeperDbContext.DepositOffers
            .Include(o => o.Conditions)
            .ThenInclude(c => c.RateLines)
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.SpecificCategories)
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.Partners)
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
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.SpecificCategories)
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.Partners)
            .FirstAsync(o => o.Id == offerModel.Id);


        // Обновляем существующую офферу
        existingOfferEf.UpdateEf(offerModel);

        // Обновляем условия и ставки
        var modelConditions = offerModel.CondsList.ToList();
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

        // Обновляем правила манибэка
        var modelRules = offerModel.MoneyBackRules.ToList();
        var modelRulesIds = modelRules.Where(r => r.Id > 0).Select(r => r.Id).ToList();

        // Удаляем правила, которых нет в модели
        var rulesToRemove = existingOfferEf.MoneyBackRules
            .Where(r => !modelRulesIds.Contains(r.Id))
            .ToList();
        foreach (var rule in rulesToRemove)
        {
            keeperDbContext.MoneyBackRules.Remove(rule);
        }

        // Обновляем или добавляем правила
        foreach (var modelRule in modelRules)
        {
            var existingRuleEf = existingOfferEf.MoneyBackRules
                .FirstOrDefault(r => r.Id == modelRule.Id);

            if (existingRuleEf != null)
            {
                // Обновляем существующее правило
                existingRuleEf.UpdateEf(modelRule);

                // Обновляем специфичные категории
                var modelCategoriesIds = modelRule.SpecificCategories.Where(c => c.Id > 0).Select(c => c.Id).ToList();
                var categoriesToRemove = existingRuleEf.SpecificCategories
                    .Where(c => !modelCategoriesIds.Contains(c.Id))
                    .ToList();
                foreach (var category in categoriesToRemove)
                {
                    keeperDbContext.MoneyBackCategories.Remove(category);
                }

                foreach (var modelCategory in modelRule.SpecificCategories)
                {
                    var existingCategoryEf = existingRuleEf.SpecificCategories
                        .FirstOrDefault(c => c.Id == modelCategory.Id);
                    if (existingCategoryEf != null)
                    {
                        existingCategoryEf.UpdateEf(modelCategory);
                    }
                    else
                    {
                        var newCategory = modelCategory.FromModel();
                        newCategory.MoneyBackRulesId = existingRuleEf.Id;
                        existingRuleEf.SpecificCategories.Add(newCategory);
                    }
                }

                // Обновляем партнёров
                var modelPartnersIds = modelRule.Partners.Where(p => p.Id > 0).Select(p => p.Id).ToList();
                var partnersToRemove = existingRuleEf.Partners
                    .Where(p => !modelPartnersIds.Contains(p.Id))
                    .ToList();
                foreach (var partner in partnersToRemove)
                {
                    keeperDbContext.MoneyBackPartners.Remove(partner);
                }

                foreach (var modelPartner in modelRule.Partners)
                {
                    var existingPartnerEf = existingRuleEf.Partners
                        .FirstOrDefault(p => p.Id == modelPartner.Id);
                    if (existingPartnerEf != null)
                    {
                        existingPartnerEf.UpdateEf(modelPartner);
                    }
                    else
                    {
                        var newPartner = modelPartner.FromModel();
                        newPartner.MoneyBackRulesId = existingRuleEf.Id;
                        existingRuleEf.Partners.Add(newPartner);
                    }
                }
            }
            else
            {
                // Добавляем новое правило используя маппер
                var newRuleEf = modelRule.FromModel();
                newRuleEf.DepositOfferId = existingOfferEf.Id;
                existingOfferEf.MoneyBackRules.Add(newRuleEf);
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
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.SpecificCategories)
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.Partners)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offerEf != null)
        {
            keeperDbContext.DepositOffers.Remove(offerEf);
            await keeperDbContext.SaveChangesAsync();
        }
    }

    // сохраняет только правила манибэка указанной офферы, не трогая условия и ставки
    public async Task SaveMoneyBackRules(int offerId, List<MoneyBackRulesModel> rules)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var existingOfferEf = await keeperDbContext.DepositOffers
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.SpecificCategories)
            .Include(o => o.MoneyBackRules)
            .ThenInclude(m => m.Partners)
            .FirstAsync(o => o.Id == offerId);

        var modelRulesIds = rules.Where(r => r.Id > 0).Select(r => r.Id).ToList();

        // Удаляем правила, которых нет в модели
        var rulesToRemove = existingOfferEf.MoneyBackRules
            .Where(r => !modelRulesIds.Contains(r.Id))
            .ToList();
        foreach (var rule in rulesToRemove)
        {
            keeperDbContext.MoneyBackRules.Remove(rule);
        }

        // для новых сущностей запоминаем связь модели с ef-сущностью,
        // чтобы после сохранения перенести назначенные базой Id обратно в модели
        var newRulesMap = new Dictionary<MoneyBackRulesModel, MoneyBackRulesEf>();
        var newCategoriesMap = new Dictionary<MoneyBackCategoryModel, MoneyBackCategoryEf>();
        var newPartnersMap = new Dictionary<MoneyBackPartnerModel, MoneyBackPartnerEf>();

        // Обновляем или добавляем правила
        foreach (var modelRule in rules)
        {
            var existingRuleEf = existingOfferEf.MoneyBackRules
                .FirstOrDefault(r => r.Id == modelRule.Id && r.Id > 0);

            if (existingRuleEf != null)
            {
                // Обновляем существующее правило
                existingRuleEf.UpdateEf(modelRule);

                // Обновляем специфичные категории
                var modelCategoriesIds = modelRule.SpecificCategories.Where(c => c.Id > 0).Select(c => c.Id).ToList();
                var categoriesToRemove = existingRuleEf.SpecificCategories
                    .Where(c => !modelCategoriesIds.Contains(c.Id))
                    .ToList();
                foreach (var category in categoriesToRemove)
                {
                    keeperDbContext.MoneyBackCategories.Remove(category);
                }

                foreach (var modelCategory in modelRule.SpecificCategories)
                {
                    var existingCategoryEf = existingRuleEf.SpecificCategories
                        .FirstOrDefault(c => c.Id == modelCategory.Id && c.Id > 0);
                    if (existingCategoryEf != null)
                    {
                        existingCategoryEf.UpdateEf(modelCategory);
                    }
                    else
                    {
                        var newCategory = modelCategory.FromModel();
                        newCategory.MoneyBackRulesId = existingRuleEf.Id;
                        existingRuleEf.SpecificCategories.Add(newCategory);
                        newCategoriesMap[modelCategory] = newCategory;
                    }
                }

                // Обновляем партнёров
                var modelPartnersIds = modelRule.Partners.Where(p => p.Id > 0).Select(p => p.Id).ToList();
                var partnersToRemove = existingRuleEf.Partners
                    .Where(p => !modelPartnersIds.Contains(p.Id))
                    .ToList();
                foreach (var partner in partnersToRemove)
                {
                    keeperDbContext.MoneyBackPartners.Remove(partner);
                }

                foreach (var modelPartner in modelRule.Partners)
                {
                    var existingPartnerEf = existingRuleEf.Partners
                        .FirstOrDefault(p => p.Id == modelPartner.Id && p.Id > 0);
                    if (existingPartnerEf != null)
                    {
                        existingPartnerEf.UpdateEf(modelPartner);
                    }
                    else
                    {
                        var newPartner = modelPartner.FromModel();
                        newPartner.MoneyBackRulesId = existingRuleEf.Id;
                        existingRuleEf.Partners.Add(newPartner);
                        newPartnersMap[modelPartner] = newPartner;
                    }
                }
            }
            else
            {
                // Добавляем новое правило используя маппер
                var newRuleEf = modelRule.FromModel();
                newRuleEf.DepositOfferId = existingOfferEf.Id;
                existingOfferEf.MoneyBackRules.Add(newRuleEf);
                newRulesMap[modelRule] = newRuleEf;
            }
        }

        await keeperDbContext.SaveChangesAsync();

        // Переносим назначенные базой Id обратно в модели
        foreach (var (modelRule, ruleEf) in newRulesMap)
        {
            modelRule.Id = ruleEf.Id;
            modelRule.DepositOfferId = ruleEf.DepositOfferId;
        }

        foreach (var (modelCategory, categoryEf) in newCategoriesMap)
        {
            modelCategory.Id = categoryEf.Id;
            modelCategory.MoneyBackRulesId = categoryEf.MoneyBackRulesId;
        }

        foreach (var (modelPartner, partnerEf) in newPartnersMap)
        {
            modelPartner.Id = partnerEf.Id;
            modelPartner.MoneyBackRulesId = partnerEf.MoneyBackRulesId;
        }
    }
}
