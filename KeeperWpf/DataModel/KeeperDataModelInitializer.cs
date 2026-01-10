using KeeperInfrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

public class KeeperDataModelInitializer(KeeperDataModel keeperDataModel,
    AccountRepository accountRepository,
    ExchangeRatesRepository exchangeRatesRepository, OfficialRatesRepository officialRatesRepository,
    MetalRatesRepository metalRatesRepository, RefinancingRatesRepository refinancingRatesRepository)
{

    public async Task GetAccountTreeFromDb()
    {
        var accounts = await accountRepository.GetAllAccounts();
        var bankAccounts = await accountRepository.GetAllBankAccounts();
        var deposits = await accountRepository.GetAllDeposits();
        var payCards = await accountRepository.GetAllPayCards();
        keeperDataModel.FillInAccountTreeAndDict(accounts, bankAccounts, deposits, payCards);
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
}
