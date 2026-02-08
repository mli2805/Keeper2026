using Caliburn.Micro;
using KeeperInfrastructure;
using KeeperModels;
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
        var repository = new DepositOffersRepository(factory);
        
        // Create test offer
        var offerModel = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
        offerModel.Title = "Offer to be deleted";
        var addedOffer = await repository.AddDepositOffer(offerModel, DbTestHelper.AcMoDict);
        
        // Setup mocks
        var windowManagerMock = new Mock<IWindowManager>();
        
        // Setup KeeperDataModel
        var dataModel = new KeeperDataModel
        {
            DepositOffers = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict),
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
        
        // Select the added offer
        viewModel.SelectedDepositOffer = viewModel.Rows.First(r => r.Id == addedOffer.Id);
        var initialCount = viewModel.Rows.Count;
        
        // Act
        await viewModel.RemoveSelectedOffer();
        
        // Assert
        Assert.HasCount(initialCount - 1, viewModel.Rows, "Offer should be removed from collection");
        Assert.IsFalse(viewModel.Rows.Any(r => r.Id == addedOffer.Id), "Removed offer should not be in collection");
        
        // Verify it's removed from database
        var offersFromDb = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
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
        var repository = new DepositOffersRepository(factory);
        
        var offers = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
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
        var oneBankOfferViewModel = new OneBankOfferViewModel(
            dataModel,
            windowManagerMock.Object,
            rulesAndRatesViewModel);
        
        // Mock dialog to return without cancellation and set the model
        windowManagerMock
            .Setup(wm => wm.ShowDialogAsync(oneBankOfferViewModel, It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()))
            .Callback(() =>
            {
                // Simulate user filling in the form
                var newOfferModel = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
                newOfferModel.Title = "New Test Offer via ViewModel";
                oneBankOfferViewModel.Initialize(newOfferModel);
                oneBankOfferViewModel.IsCancelled = false;
            })
            .Returns(Task.FromResult<bool?>(true));
        
        // Create viewModel
        var viewModel = new BankOffersViewModel(
            windowManagerMock.Object,
            dataModel,
            repository,
            oneBankOfferViewModel);
        
        viewModel.Initialize();
        var initialViewModelCount = viewModel.Rows.Count;
        
        // Act
        await viewModel.AddOffer();
        
        // Assert
        Assert.HasCount(initialViewModelCount + 1, viewModel.Rows, "Offer should be added to collection");
        Assert.IsTrue(viewModel.Rows.Any(r => r.Title == "New Test Offer via ViewModel"), "New offer should be in collection");
        
        // Verify it's in database
        var offersFromDb = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        Assert.HasCount(initialCount + 1, offersFromDb, "Offer should be added to database");
        Assert.IsTrue(offersFromDb.Any(o => o.Title == "New Test Offer via ViewModel"), "New offer should be in database");
        
        // Verify the new offer is selected
        Assert.IsNotNull(viewModel.SelectedDepositOffer);
        Assert.AreEqual("New Test Offer via ViewModel", viewModel.SelectedDepositOffer.Title);
    }

    [TestMethod]
    public async Task EditSelectedOffer_ShouldUpdateInDbAndCollection()
    {
        // Arrange
        var factory = DbTestHelper.CreateIsolatedFactory();
        var repository = new DepositOffersRepository(factory);
        
        // Create test offer
        var offerModel = DepositOfferTestHelper.CreateDepositOfferModel(DbTestHelper.AcMoDict);
        offerModel.Title = "Original Test Offer";
        var addedOffer = await repository.AddDepositOffer(offerModel, DbTestHelper.AcMoDict);
        
        // Setup mocks
        var windowManagerMock = new Mock<IWindowManager>();
        
        // Setup KeeperDataModel
        var dataModel = new KeeperDataModel
        {
            DepositOffers = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict),
            AcMoDict = DbTestHelper.AcMoDict
        };
        
        var rulesAndRatesViewModel = new RulesAndRatesViewModel(dataModel, windowManagerMock.Object);
        var oneBankOfferViewModel = new OneBankOfferViewModel(
            dataModel,
            windowManagerMock.Object,
            rulesAndRatesViewModel);
        
        // Mock dialog to modify the model and return without cancellation
        windowManagerMock
            .Setup(wm => wm.ShowDialogAsync(oneBankOfferViewModel, It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()))
            .Callback(() =>
            {
                // Simulate user editing the form
                oneBankOfferViewModel.ModelInWork.Title = "Updated Test Offer";
                oneBankOfferViewModel.ModelInWork.MonthPaymentsMinimum = 500;
                oneBankOfferViewModel.IsCancelled = false;
            })
            .Returns(Task.FromResult<bool?>(true));
        
        // Create viewModel
        var viewModel = new BankOffersViewModel(
            windowManagerMock.Object,
            dataModel,
            repository,
            oneBankOfferViewModel);
        
        viewModel.Initialize();
        viewModel.SelectedDepositOffer = viewModel.Rows.First(r => r.Id == addedOffer.Id);
        
        // Act
        await viewModel.EditSelectedOffer();
        
        // Assert
        var updatedInCollection = viewModel.Rows.First(r => r.Id == addedOffer.Id);
        Assert.AreEqual("Updated Test Offer", updatedInCollection.Title, "Title should be updated in collection");
        Assert.AreEqual(500, updatedInCollection.MonthPaymentsMinimum, "MonthPaymentsMinimum should be updated");
        
        // Verify it's updated in database
        var offersFromDb = await repository.GetDepositOffersWithConditionsAndRates(DbTestHelper.AcMoDict);
        var updatedInDb = offersFromDb.First(o => o.Id == addedOffer.Id);
        Assert.AreEqual("Updated Test Offer", updatedInDb.Title, "Title should be updated in database");
        Assert.AreEqual(500, updatedInDb.MonthPaymentsMinimum, "MonthPaymentsMinimum should be updated in database");
        
        // Verify the updated offer is still selected
        Assert.AreEqual(addedOffer.Id, viewModel.SelectedDepositOffer.Id);
    }
}
