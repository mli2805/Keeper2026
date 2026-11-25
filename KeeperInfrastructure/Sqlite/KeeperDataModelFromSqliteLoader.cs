using System.Diagnostics;
using KeeperModels;

namespace KeeperInfrastructure;

/// <summary>
/// Модели с которыми работает WPF приложение находятся в отдельном проекте KeeperModels (wpf class library)
/// EF entities + DbContext + Repositories в проекте KeeperInfrastructure
/// Мапперы ToEf/FromEf для сущностей в KeeperDomain (старая программа и бэкап)  
/// в проекте KeeperInfrastructure/Sqlite/Mappers
/// Мапперы из EF entities в модели зашиты в методы репозиториев в проекте KeeperInfrastructure
/// 
/// Здесь вызываем методы репозиториев для загрузки данных из БД в общую модель KeeperDataModel
/// Инитим один раз при старте приложения, т.к. данные сильно связанны между собой и для большинства форм
/// нужны как минимум счета, транзакции и курсы
/// </summary>
public class KeeperDataModelFromSqliteLoader(KeeperDataModel keeperDataModel,
    AccountRepository accountRepository, CarRepository carRepository, DepositOffersRepository depositOffersRepository,
    ExchangeRatesRepository exchangeRatesRepository, OfficialRatesRepository officialRatesRepository,
    MetalRatesRepository metalRatesRepository, RefinancingRatesRepository refinancingRatesRepository,
    TrustAccountsRepository trustAccountsRepository, TrustAssetsRepository trustAssetsRepository,
    TrustAssetRatesRepository trustAssetRatesRepository, TrustTransactionsRepository trustTransactionsRepository,
    TransactionsRepository transactionsRepository, FuellingsRepository fuellingsRepository,
    BankAccountMemosRepository bankAccountMemosRepository,
    CustomRemindersRepository customRemindersRepository,
    LargeExpenseThresholdsRepository largeExpenseThresholdsRepository,
    ButtonCollectionsRepository buttonCollectionsRepository, SalaryChangesRepository salaryChangesRepository,
    SalaryPaymentsRepository salaryPaymentsRepository,
    TodoTaskRepository todoTaskRepository, HouseholdRepository householdRepository)
{
    public async Task<bool> GetFullModelFromDb()
    {
        var success = await GetAccountsTreeAndDictFromDb();
        if (!success)
        {
            // на случай если БД пустая, только что удалили и создали новую (будем грузить из текстового бэкапа)
            return false;
        }
        await GetRatesFromDb();
        await GetTransactionsFromDb();
        keeperDataModel.Cars = await carRepository.GetAllCarsWithMileages();
        keeperDataModel.TodoTasks = await todoTaskRepository.GetAllTodoTasksWithSubtasks();
        keeperDataModel.DepositOffers = await depositOffersRepository.GetDepositOffersWithConditionsAndRates(keeperDataModel.AcMoDict);
        await GetTrustDataFromDb(keeperDataModel.AcMoDict);
        await GetOthersFromDb();
        return true;
    }

    private async Task<bool> GetAccountsTreeAndDictFromDb()
    {
        var pair = await accountRepository.GetAccountModelsTreeAndDict();
        //  со счетов начинаем поэтому добавил проверку на пустую базу
        if (pair == null)
        {
            return false;
        }
        keeperDataModel.AccountsTree = pair.Value.Item1;
        keeperDataModel.AcMoDict = pair.Value.Item2;
        return true;
    }

    private Task GetRatesFromDb()
    {
        // так и показываем на вью
        keeperDataModel.ExchangeRates = exchangeRatesRepository.GetAllExchangeRates().ToDictionary(r => r.Date);
        // потом на вью преобразуем в OfficialRatesModel (долгое преобразование с дельтами, в отдельном потоке)
        keeperDataModel.OfficialRates = officialRatesRepository.GetAllOfficialRates().ToDictionary(r => r.Date);
        // на вью преобразуем в GoldCoinsModel (это можно было бы делать в репозитории, вместо FromEf)
        keeperDataModel.MetalRates = metalRatesRepository.GetAllMetalRates();
        // так и показываем на вью
        keeperDataModel.RefinancingRates = refinancingRatesRepository.GetAll();
        Debug.WriteLine($"Loaded {keeperDataModel.ExchangeRates.Count} exchange rates from DB");
        return Task.CompletedTask;
    }

    private async Task GetTrustDataFromDb(Dictionary<int, AccountItemModel> acMoDict)
    {
        keeperDataModel.TrustAccounts = await trustAccountsRepository.GetAllTrustAccounts();
        keeperDataModel.InvestmentAssets = await trustAssetsRepository.GetAllTrustAssetModels(keeperDataModel.TrustAccounts);
        keeperDataModel.AssetRates = trustAssetRatesRepository.GetAllTrustAssetRates();
        keeperDataModel.InvestTranModels = await trustTransactionsRepository
            .GetAllTrustTransactionModels(acMoDict, keeperDataModel.TrustAccounts, keeperDataModel.InvestmentAssets);
    }

    private async Task GetTransactionsFromDb()
    {
        keeperDataModel.Transactions = (await transactionsRepository
            .GetAllTransactionModels(keeperDataModel.AcMoDict)).ToDictionary(t => t.Id);

        keeperDataModel.FuellingVms = fuellingsRepository.GetAllFuellingModels(keeperDataModel);
    }

    private async Task GetOthersFromDb()
    {
        keeperDataModel.SalaryChanges = await salaryChangesRepository.GetAllSalaryChanges();
        keeperDataModel.SalaryPayments = await salaryPaymentsRepository.GetAllSalaryPayments();
        keeperDataModel.BankAccountMemoModels = 
            await bankAccountMemosRepository.GetAllBankAccountMemos(keeperDataModel.AcMoDict);
        keeperDataModel.CustomReminderModels = 
            await customRemindersRepository.GetAllCustomReminders();
        keeperDataModel.LargeExpenseThresholds = 
            await largeExpenseThresholdsRepository.GetAllLargeExpenseThresholds();
        keeperDataModel.ButtonCollections = 
            await buttonCollectionsRepository.GetAllButtonCollections(keeperDataModel.AcMoDict);
        keeperDataModel.UtilityMeters = await householdRepository.GetAllUtilityMeters();
    }
}
