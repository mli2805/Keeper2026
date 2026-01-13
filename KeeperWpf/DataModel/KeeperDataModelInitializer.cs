using KeeperInfrastructure;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

public class KeeperDataModelInitializer(KeeperDataModel keeperDataModel,
    AccountRepository accountRepository,
    ExchangeRatesRepository exchangeRatesRepository, OfficialRatesRepository officialRatesRepository,
    MetalRatesRepository metalRatesRepository, RefinancingRatesRepository refinancingRatesRepository,
    TrustAccountsRepository trustAccountsRepository, TrustAssetsRepository trustAssetsRepository,
    TrustAssetRatesRepository trustAssetRatesRepository, TrustTransactionsRepository trustTransactionsRepository,
    TransactionsRepository transactionsRepository, FuellingsRepository fuellingsRepository)
{

    public async Task<bool> GetAccountTreeAndDictionaryFromDb()
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

    public void GetRatesFromDb()
    {
        keeperDataModel.ExchangeRates = exchangeRatesRepository.GetAllExchangeRates().ToDictionary(r => r.Date);
        keeperDataModel.OfficialRates = officialRatesRepository.GetAllOfficialRates().ToDictionary(r => r.Date);
        keeperDataModel.MetalRates = metalRatesRepository.GetAllMetalRates();
        keeperDataModel.RefinancingRates = refinancingRatesRepository.GetAllRefinancingRates();
        Debug.WriteLine($"Loaded {keeperDataModel.ExchangeRates.Count} exchange rates from DB");
    }

    public void GetDepositOffersFromDb()
    {

    }

    public void GetTrustDataFromDb()
    {
        keeperDataModel.TrustAccounts = trustAccountsRepository.GetAllTrustAccounts();
        keeperDataModel.InvestmentAssets = trustAssetsRepository
            .GetAllTrustAssets().Select(a => a.ToModel(keeperDataModel)).ToList();
        keeperDataModel.AssetRates = trustAssetRatesRepository.GetAllTrustAssetRates();
        keeperDataModel.InvestTranModels = trustTransactionsRepository
            .GetAllTrustTransactions().Select(t => t.ToModel(keeperDataModel)).ToList();
    }

    public void GetTransactionsFromDb()
    {
        keeperDataModel.Transactions = transactionsRepository
            .GetAllTransactions().Select(t => t.ToModel(keeperDataModel.AcMoDict))
            .ToDictionary(t => t.Id, t => t);

        var fuellings = fuellingsRepository.GetAllFuellings();
        keeperDataModel.FuellingJoinTransaction(fuellings);
    }
}
