using KeeperInfrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

public class KeeperDataModelInitializer(KeeperDataModel keeperDataModel,
    AccountRepository accountRepository,
    ExchangeRatesRepository exchangeRatesRepository, OfficialRatesRepository officialRatesRepository,
    MetalRatesRepository metalRatesRepository, RefinancingRatesRepository refinancingRatesRepository,
    TransactionsRepository transactionsRepository)
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

    public void GetExchangeRatesFromDb()
    {
        var exchangeRates = exchangeRatesRepository.GetAllExchangeRates();
        var dict = exchangeRates.ToDictionary(r => r.Date);
        keeperDataModel.ExchangeRates = dict;
    }

    public void GetOfficialRatesFromDb()
    {
        var officialRates = officialRatesRepository.GetAllOfficialRates();
        var dict = officialRates.ToDictionary(r => r.Date);
        keeperDataModel.OfficialRates = dict;
    }

    public void GetMetalRatesFromDb()
    {
        var metalRates = metalRatesRepository.GetAllMetalRates();
        keeperDataModel.MetalRates = metalRates;
    }

    public void GetRefinancingRatesFromDb()
    {
        var refinancingRates = refinancingRatesRepository.GetAllRefinancingRates();
        keeperDataModel.RefinancingRates = refinancingRates;
    }

    public void GetTransactionsFromDb()
    {
        var transactions = transactionsRepository.GetAllTransactions();
        keeperDataModel.Transactions = transactions.Select(t=>t.ToModel(keeperDataModel.AcMoDict)).ToDictionary(t=>t.Id, t=>t);
    }
}
