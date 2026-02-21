using KeeperInfrastructure;
using KeeperModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

/// <summary>
/// Модели с которыми работает WPF приложение находятся в отдельном проекте KeeperModels (wpf class library)
/// EF entities + DbContext + Repositories в проекте KeeperInfrastructure
/// Мапперы ToEf/FromEf для сущностей в KeeperDomain (старая программа и бэкап)  в проекте KeeperInfrastructure/Sqlite/Mappers
/// Мапперы из EF entities в модели зашиты в методы репозиториев в проекте KeeperInfrastructure
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
        var success = await GetAccountsTreeAndDictFromDb();
        if (!success)
        {
            // на случай если БД пустая, только что удалили и создали новую (будем грузить из текстового бэкапа)
            return false;
        }
        GetRatesFromDb();
        await GetTransactionsFromDb();
        keeperDataModel.Cars = await carRepository.GetAllCarsWithMileages();
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
        keeperDataModel.AccountsTree = pair!.Value.Item1;
        keeperDataModel.AcMoDict = pair!.Value.Item2;
        return true;
    }

    private void GetRatesFromDb()
    {
        // так и показываем на вью
        keeperDataModel.ExchangeRates = exchangeRatesRepository.GetAllExchangeRates().ToDictionary(r => r.Date);
        // на вью преобразуем в OfficialRatesModel (долгое преобразование с дельтами, в отдельном потоке)
        keeperDataModel.OfficialRates = officialRatesRepository.GetAllOfficialRates().ToDictionary(r => r.Date);
        // на вью преобразуем в GoldCoinsModel (это можно было бы делать в репозитории, вместо FromEf)
        keeperDataModel.MetalRates = metalRatesRepository.GetAllMetalRates();
        // так и показываем на вью
        keeperDataModel.RefinancingRates = refinancingRatesRepository.GetAllRefinancingRates();
        Debug.WriteLine($"Loaded {keeperDataModel.ExchangeRates.Count} exchange rates from DB");
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

        var fuellings = fuellingsRepository.GetAllFuellings();
        // тут не только маппинг, но и вычисление доп. полей в моделях
        // наверное стоит это выполнять только при необходимости, когда открывается форма заправок или ввода новой заправки
        keeperDataModel.FuellingJoinTransaction(fuellings);
    }

    private async Task GetOthersFromDb()
    {
        keeperDataModel.SalaryChanges = await salaryChangesRepository.GetAllSalaryChanges();
        keeperDataModel.CardBalanceMemoModels = await cardBalanceMemosRepository.GetAllCardBalanceMemos(keeperDataModel.AcMoDict);
        keeperDataModel.LargeExpenseThresholds = largeExpenseThresholdsRepository.GetAllLargeExpenseThresholds();
        keeperDataModel.ButtonCollections = await buttonCollectionsRepository.GetAllButtonCollections(keeperDataModel.AcMoDict);
    }
}
