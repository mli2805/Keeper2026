using Caliburn.Micro;
using KeeperInfrastructure;
using KeeperWpf;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Keeper;

[TestClass]
public sealed class BankOffersViewModelTests
{
    [TestMethod]
    public async Task RemoveSelectedOffer_WhenOfferHasNoDeposits_ShouldRemoveFromDbAndCollection()
    {
        // Arrange
        var factory = DbTestHelper.CreateIsolatedFactory();
        var depositOffersRepository = new DepositOffersRepository(factory);

        // создаем новый офер без депозитов, чтобы можно было его удалить
        var addedOffer = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
        addedOffer = await depositOffersRepository.AddDepositOffer(addedOffer, DbTestHelper.AcMoDict);

        // Setup mocks
        var windowManagerMock = new Mock<IWindowManager>();

        // Setup KeeperDataModel
        var dataModel = new KeeperDataModel
        {
            DepositOffers = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict),
            AcMoDict = DbTestHelper.AcMoDict
        };

        var rulesAndRatesViewModel = new RulesAndRatesViewModel(dataModel, windowManagerMock.Object);
        var oneBankOfferViewModel = new OneBankOfferViewModel(dataModel, windowManagerMock.Object, rulesAndRatesViewModel);

        // Create viewModel
        var viewModel = new BankOffersViewModel(windowManagerMock.Object, dataModel, depositOffersRepository, oneBankOfferViewModel);
        viewModel.Initialize();

        // селектим добавленный оффер 
        viewModel.SelectedDepositOffer = viewModel.Rows.First(r => r.Id == addedOffer.Id);
        var initialCount = viewModel.Rows.Count;

        // Act
        await viewModel.RemoveSelectedOffer();

        // Assert
        Assert.HasCount(initialCount - 1, viewModel.Rows, "Offer should be removed from collection");
        Assert.IsFalse(viewModel.Rows.Any(r => r.Id == addedOffer.Id), "Removed offer should not be in collection");

        // Verify it's removed from database
        var offersFromDb = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        Assert.IsFalse(offersFromDb.Any(o => o.Id == addedOffer.Id), "Offer should be removed from database");
    }

    [TestMethod]
    public async Task RemoveSelectedOffer_WhenOfferHasDeposits_ShouldShowErrorAndNotRemove()
    {
        // Arrange
        var factory = DbTestHelper.CreateIsolatedFactory();
        var repository = new DepositOffersRepository(factory);

        // Get offers from database (assuming there's at least one with deposits)
        var offers = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        var offerWithDeposit = offers.FirstOrDefault(o =>
            DbTestHelper.AcMoDict.Values.Any(a => a.IsDeposit && a.BankAccount?.DepositOfferId == o.Id));

        if (offerWithDeposit == null)
        {
            Assert.Inconclusive("No offer with deposits found in test database");
            return;
        }

        // Setup mocks
        var windowManagerMock = new Mock<IWindowManager>();

        // Setup to capture ShowDialogAsync call
        MyMessageBoxViewModel? capturedViewModel = null;
        windowManagerMock
            .Setup(wm => wm.ShowDialogAsync(It.IsAny<MyMessageBoxViewModel>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()))
            .Callback<object, object, IDictionary<string, object>>((vm, context, settings) =>
            {
                capturedViewModel = vm as MyMessageBoxViewModel;
            })
            .Returns(Task.FromResult<bool?>(true));

        // Setup KeeperDataModel
        var dataModel = new KeeperDataModel
        {
            DepositOffers = offers,
            AcMoDict = DbTestHelper.AcMoDict
        };

        var rulesAndRatesViewModel = new RulesAndRatesViewModel(dataModel, windowManagerMock.Object);
        var oneBankOfferViewModel = new OneBankOfferViewModel(
            dataModel,
            windowManagerMock.Object,
            rulesAndRatesViewModel);

        // Create viewModel
        var viewModel = new BankOffersViewModel(
            windowManagerMock.Object,
            dataModel,
            repository,
            oneBankOfferViewModel);

        viewModel.Initialize();
        viewModel.SelectedDepositOffer = viewModel.Rows.First(r => r.Id == offerWithDeposit.Id);
        var initialCount = viewModel.Rows.Count;

        // Act
        await viewModel.RemoveSelectedOffer();

        // Assert
        Assert.HasCount(initialCount, viewModel.Rows, "Offer should NOT be removed from collection");
        Assert.IsTrue(viewModel.Rows.Any(r => r.Id == offerWithDeposit.Id), "Offer should still be in collection");

        // Verify error dialog was shown
        windowManagerMock.Verify(wm => wm.ShowDialogAsync(
            It.IsAny<MyMessageBoxViewModel>(),
            It.IsAny<object>(),
            It.IsAny<IDictionary<string, object>>()), Times.Once);

        Assert.IsNotNull(capturedViewModel, "Error message box should have been shown");

        // Verify it's still in database
        var offersFromDb = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        Assert.IsTrue(offersFromDb.Any(o => o.Id == offerWithDeposit.Id), "Offer should still be in database");
    }

    [TestMethod]
    public async Task AddOffer_ShouldAddToDbAndCollection()
    {
        // Arrange
        var factory = DbTestHelper.CreateIsolatedFactory();
        var depositOffersRepository = new DepositOffersRepository(factory);

        var offers = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        var initialCount = offers.Count;

        // Setup mocks
        var windowManagerMock = new Mock<IWindowManager>();

        // Setup KeeperDataModel
        var dataModel = new KeeperDataModel
        {
            DepositOffers = offers,
            AcMoDict = DbTestHelper.AcMoDict
        };

        var rulesAndRatesViewModel = new RulesAndRatesViewModel(dataModel, windowManagerMock.Object);
        var oneBankOfferViewModel = new OneBankOfferViewModel(dataModel, windowManagerMock.Object, rulesAndRatesViewModel);

        // Mock dialog to return without cancellation and set the model
        var newOfferModel = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
        var newOfferTitle = newOfferModel.Title;
        windowManagerMock
            .Setup(wm => wm.ShowDialogAsync(oneBankOfferViewModel, It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()))
            .Callback(() =>
            {
                // Simulate user filling in the form
                
                oneBankOfferViewModel.Initialize(newOfferModel);
                oneBankOfferViewModel.IsCancelled = false;
            })
            .Returns(Task.FromResult<bool?>(true));

        // Create viewModel
        var viewModel = new BankOffersViewModel(windowManagerMock.Object, dataModel, depositOffersRepository, oneBankOfferViewModel);
        viewModel.Initialize();
        var initialViewModelCount = viewModel.Rows.Count;

        // Act
        await viewModel.AddOffer();

        // Assert
        Assert.HasCount(initialViewModelCount + 1, viewModel.Rows, "Offer should be added to collection");
        Assert.IsTrue(viewModel.Rows.Any(r => r.Title == newOfferTitle), "New offer should be in collection");

        // Verify it's in database
        var offersFromDb = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        Assert.HasCount(initialCount + 1, offersFromDb, "Offer should be added to database");
        Assert.IsTrue(offersFromDb.Any(o => o.Title == newOfferTitle), "New offer should be in database");

        // Verify the new offer is selected
        Assert.IsNotNull(viewModel.SelectedDepositOffer);
        Assert.AreEqual(newOfferTitle, viewModel.SelectedDepositOffer.Title);
    }

    [TestMethod]
    public async Task EditSelectedOffer_ShouldUpdateInDbAndCollection()
    {
        // Arrange
        var factory = DbTestHelper.CreateIsolatedFactory();
        var depositOffersRepository = new DepositOffersRepository(factory);

        // Create test offer
        var offerModel = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
        var addedOffer = await depositOffersRepository.AddDepositOffer(offerModel, DbTestHelper.AcMoDict);

        // Setup mocks
        var windowManagerMock = new Mock<IWindowManager>();

        // Setup KeeperDataModel
        var dataModel = new KeeperDataModel
        {
            DepositOffers = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict),
            AcMoDict = DbTestHelper.AcMoDict
        };

        var rulesAndRatesViewModel = new RulesAndRatesViewModel(dataModel, windowManagerMock.Object);
        var oneBankOfferViewModel = new OneBankOfferViewModel(dataModel, windowManagerMock.Object, rulesAndRatesViewModel);

        // Mock dialog to modify the model and return without cancellation
        windowManagerMock
            .Setup(wm => wm.ShowDialogAsync(oneBankOfferViewModel, It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()))
            .Callback(() =>
            {
                // Simulate user editing the form
                oneBankOfferViewModel.ModelInWork = DepositOfferTestHelper.ChangeDepositOfferModel(addedOffer);
                oneBankOfferViewModel.IsCancelled = false;
            })
            .Returns(Task.FromResult<bool?>(true));

        // Create viewModel
        var viewModel = new BankOffersViewModel(windowManagerMock.Object, dataModel, depositOffersRepository, oneBankOfferViewModel);
        viewModel.Initialize();
        viewModel.SelectedDepositOffer = viewModel.Rows.First(r => r.Id == addedOffer.Id);

        // Act
        await viewModel.EditSelectedOffer();

        // Assert
        var updatedInCollection = viewModel.Rows.First(r => r.Id == addedOffer.Id);
        Assert.AreEqual("Обновленный тестовый вклад", updatedInCollection.Title, "Title should be updated in collection");
        Assert.AreEqual(200, updatedInCollection.MonthPaymentsMinimum, "MonthPaymentsMinimum should be updated");

        // Verify it's updated in database
        var offersFromDb = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        var updatedInDb = offersFromDb.First(o => o.Id == addedOffer.Id);
        Assert.AreEqual("Обновленный тестовый вклад", updatedInDb.Title, "Title should be updated in database");
        Assert.AreEqual(200, updatedInDb.MonthPaymentsMinimum, "MonthPaymentsMinimum should be updated in database");

        var conds1 = updatedInDb.CondsMap[DateTime.Today.AddDays(-30)];
        Assert.IsNotNull(conds1);
        Assert.IsFalse(conds1.IsFactDays);
        Assert.HasCount(3, conds1.RateLines);
        Assert.AreEqual(4.5m, conds1.RateLines[1].Rate);

        var conds2 = updatedInDb.CondsMap[DateTime.Today.AddDays(-15)];
        Assert.IsNotNull(conds2);
        Assert.HasCount(2, conds2.RateLines);

        Assert.IsFalse(updatedInDb.CondsMap.ContainsKey(DateTime.Today));

        // Verify the updated offer is still selected
        Assert.AreEqual(addedOffer.Id, viewModel.SelectedDepositOffer.Id);
    }
}
