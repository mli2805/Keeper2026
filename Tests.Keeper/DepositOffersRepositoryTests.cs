using KeeperInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Tests.Keeper;

[TestClass]
public sealed class DepositOffersRepositoryTests
{
    [TestMethod]
    public async Task ReadDepositOffersTest()
    {
        IDbContextFactory<KeeperDbContext> factory = DbTestHelper.CreateIsolatedFactory();
        var repository = new DepositOffersRepository(factory);
        var offers = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        Assert.IsNotNull(offers);
        Assert.IsNotEmpty(offers, "Ожидалось, что в базе данных будет хотя бы одна оффера.");
    }

    [TestMethod]
    public async Task AddDepositOfferTest()
    {
        IDbContextFactory<KeeperDbContext> factory = DbTestHelper.CreateIsolatedFactory();
        var repository = new DepositOffersRepository(factory);
        var offers = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        var initialCount = offers.Count;

        var offerModel = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
        var savedOfferModel = await repository.AddDepositOffer(offerModel, DbTestHelper.AcMoDict);
        var offers2 = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        Assert.HasCount(expected: initialCount + 1, offers2);
        var addedOffer = offers2.FirstOrDefault(o => o.Id == savedOfferModel.Id);

        Assert.IsNotNull(addedOffer);
        Assert.AreEqual(offerModel.MonthPaymentsMinimum, addedOffer!.MonthPaymentsMinimum);
        Assert.HasCount(expected: offerModel.CondsMap.Count, addedOffer.CondsMap);
    }

    [TestMethod]
    public async Task UpdateDepositOfferTest()
    {
        // Arrange
        IDbContextFactory<KeeperDbContext> factory = DbTestHelper.CreateIsolatedFactory();
        var repository = new DepositOffersRepository(factory);
        var offerModel = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
        var addedOfferModel = await repository.AddDepositOffer(offerModel, DbTestHelper.AcMoDict);

        // Act - Update title
        var changedModel = DepositOfferTestHelper.ChangeDepositOfferModel(addedOfferModel);
        var updatedOfferModel = await repository.UpdateDepositOffer(changedModel, DbTestHelper.AcMoDict);

        // Assert
        var offers = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        var updatedOffer = offers.FirstOrDefault(o => o.Id == addedOfferModel.Id);
        Assert.IsNotNull(updatedOffer);
        Assert.AreEqual("Обновленный тестовый вклад", updatedOffer!.Title);
        Assert.AreEqual(offerModel.MonthPaymentsMinimum + 100, updatedOffer.MonthPaymentsMinimum);
        var conds1 = updatedOffer.CondsMap[DateTime.Today.AddDays(-30)];
        Assert.IsNotNull(conds1);
        Assert.IsFalse(conds1.IsFactDays);
        Assert.HasCount(3, conds1.RateLines);
        Assert.AreEqual(4.5m, conds1.RateLines[1].Rate);

        var conds2 = updatedOffer.CondsMap[DateTime.Today.AddDays(-15)];
        Assert.IsNotNull(conds2);
        Assert.HasCount(2, conds2.RateLines);

        Assert.IsFalse(updatedOffer.CondsMap.ContainsKey(DateTime.Today));
    }
}
