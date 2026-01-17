using KeeperInfrastructure;
using KeeperModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

/// <summary>
/// Модели с которыми работает WPF приложение в отдельном проекте KeeperModels (wpf class library)
/// EF entities + DbContext + Repositories в проекте KeeperInfrastructure
/// Мапперы из EF entities в модели зашиты в методы репозиториев в проекте KeeperInfrastructure
/// Мапперы ToEf/FromEf для сущностей в KeeperDomain (старая программа и бэкап)  в проекте KeeperInfrastructure/Sqlite/Mappers
/// 
/// Здесь вызываем методы репозиториев для загрузки данных из БД в общую модель KeeperDataModel
/// Инитим один раз при старте приложения, т.к. данные сильно связанны между собой и для большинства форм
/// нужны как минимум счета, транзакции и курсы
/// </summary>
public class KeeperDataModelInitializer(KeeperDataModel keeperDataModel,
    AccountRepository accountRepository, CarRepository carRepository, DepositOffersRepository depositOffersRepository,
    ExchangeRatesRepository exchangeRatesRepository, OfficialRatesRepository officialRatesRepository,
    MetalRatesRepository metalRatesRepository, RefinancingRatesRepository refinancingRatesRepository,
    TrustAccountsRepository trustAccountsRepository, TrustAssetsRepository trustAssetsRepository,
    TrustAssetRatesRepository trustAssetRatesRepository, TrustTransactionsRepository trustTransactionsRepository,
    TransactionsRepository transactionsRepository, FuellingsRepository fuellingsRepository,
    CardBalanceMemosRepository cardBalanceMemosRepository, LargeExpenseThresholdsRepository largeExpenseThresholdsRepository,
    ButtonCollectionsRepository buttonCollectionsRepository, SalaryChangesRepository salaryChangesRepository)
{
    public async Task<bool> GetFullModelFromDb()
    {
        var success = await GetAccountTreeAndDictionaryFromDb();
        if (!success)
        {
            // на случай если БД пустая, только что удалили и создали новую (будем грузить из текстового бэкапа)
            return false;
        }
        GetRatesFromDb();
        GetTransactionsFromDb();
        await GetCarsFromDb();
        await GetDepositOffersFromDb(keeperDataModel.AcMoDict);
        GetTrustDataFromDb();
        await GetOthersFromDb();
        return true;
    }

    private async Task<bool> GetAccountTreeAndDictionaryFromDb()
    {
        // со счетов начинаем поэтому добавил проверку на наличие счетов
        var accounts = await accountRepository.GetAllAccounts();
        if (accounts == null || accounts.Count == 0)
        {
            return false;
        }
        var bankAccounts = await accountRepository.GetAllBankAccounts();
        var deposits = await accountRepository.GetAllDeposits();
        var payCards = await accountRepository.GetAllPayCards();
        keeperDataModel.FillInAccountTreeAndDict(accounts, bankAccounts, deposits, payCards);
        return true;
    }

    private void GetRatesFromDb()
    {
        keeperDataModel.ExchangeRates = exchangeRatesRepository.GetAllExchangeRates().ToDictionary(r => r.Date);
        keeperDataModel.OfficialRates = officialRatesRepository.GetAllOfficialRates().ToDictionary(r => r.Date);
        keeperDataModel.MetalRates = metalRatesRepository.GetAllMetalRates();
        keeperDataModel.RefinancingRates = refinancingRatesRepository.GetAllRefinancingRates();
        Debug.WriteLine($"Loaded {keeperDataModel.ExchangeRates.Count} exchange rates from DB");
    }

    private async Task GetCarsFromDb()
    {
        keeperDataModel.Cars = await carRepository.GetAllCarsWithMileages();
    }


    private async Task GetDepositOffersFromDb(Dictionary<int, AccountItemModel> acMoDict)
    {
        keeperDataModel.DepositOffers = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(acMoDict);
    }

    private void GetTrustDataFromDb()
    {
        keeperDataModel.TrustAccounts = trustAccountsRepository.GetAllTrustAccounts();
        keeperDataModel.InvestmentAssets = trustAssetsRepository
            .GetAllTrustAssets().Select(a => a.ToModel(keeperDataModel)).ToList();
        keeperDataModel.AssetRates = trustAssetRatesRepository.GetAllTrustAssetRates();
        keeperDataModel.InvestTranModels = trustTransactionsRepository
            .GetAllTrustTransactions().Select(t => t.ToModel(keeperDataModel)).ToList();
    }

    private void GetTransactionsFromDb()
    {
        keeperDataModel.Transactions = transactionsRepository
            .GetAllTransactions().Select(t => t.ToModel(keeperDataModel.AcMoDict))
            .ToDictionary(t => t.Id, t => t);

        var fuellings = fuellingsRepository.GetAllFuellings();
        keeperDataModel.FuellingJoinTransaction(fuellings);
    }

    private async Task GetOthersFromDb()
    {
        keeperDataModel.SalaryChanges = salaryChangesRepository.GetAllSalaryChanges();
        keeperDataModel.CardBalanceMemoModels = await cardBalanceMemosRepository.GetAllCardBalanceMemos(keeperDataModel.AcMoDict);
        keeperDataModel.LargeExpenseThresholds = largeExpenseThresholdsRepository.GetAllLargeExpenseThresholds();
        keeperDataModel.ButtonCollections = await buttonCollectionsRepository.GetAllButtonCollections(keeperDataModel.AcMoDict);
    }
}
