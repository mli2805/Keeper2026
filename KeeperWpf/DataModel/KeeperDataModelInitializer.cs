using KeeperInfrastructure;
using System.Linq;

namespace KeeperWpf;

public class KeeperDataModelInitializer(KeeperDataModel keeperDataModel,
    ExchangeRatesRepository exchangeRatesRepository, OfficialRatesRepository officialRatesRepository,
    MetalRatesRepository metalRatesRepository, RefinancingRatesRepository refinancingRatesRepository)
{
    public void InitializeExchangeRates()
    {
        var exchangeRates = exchangeRatesRepository.GetAllExchangeRates();
        var dict = exchangeRates.ToDictionary(r => r.Date);
        keeperDataModel.ExchangeRates = dict;
    }

    public void InitializeOfficialRates()
    {
        var officialRates = officialRatesRepository.GetAllOfficialRates();
        var dict = officialRates.ToDictionary(r => r.Date);
        keeperDataModel.OfficialRates = dict;
    }

    public void InitializeMetalRates()
    {
        var metalRates = metalRatesRepository.GetAllMetalRates();
        keeperDataModel.MetalRates = metalRates;
    }

    public void InitializeRefinancingRates()
    {
        var refinancingRates = refinancingRatesRepository.GetAllRefinancingRates();
        keeperDataModel.RefinancingRates = refinancingRates;
    }
}
