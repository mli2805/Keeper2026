using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;

namespace KeeperWpf;

[ExportViewModel]
public class RefinancingRatesViewModel(KeeperDataModel keeperDataModel, 
    RefinancingRatesRepository refinancingRatesRepository) : PropertyChangedBase
{
    public ObservableCollection<RefinancingRate> Rows { get; set; } = null!;

    public void Initialize()
    {
        Rows = new ObservableCollection<RefinancingRate>(keeperDataModel.RefinancingRates);
    }

    public async void Download()
    {
        var rates = await NbRbRatesDownloader.GetRefinanceRatesAsync();
        if (rates == null) return;

        if (rates.Count > keeperDataModel.RefinancingRates.Count)
        {
            var lastId = 0;
            foreach (var refinancingRate in rates.OrderBy(r => r.Date))
            {
                var dr = keeperDataModel.RefinancingRates.FirstOrDefault(r => r.Date == refinancingRate.Date);
                if (dr != null)
                    lastId = dr.Id;
                else
                {
                    refinancingRate.Id = ++lastId;
                    keeperDataModel.RefinancingRates.Add(refinancingRate);
                    Rows.Add(refinancingRate);
                    refinancingRatesRepository.Add(refinancingRate);
                }
            }
        }
    }

    public void UpdateRates()
    {
        keeperDataModel.UpdateDepoRatesLinkedToCp();
    }

}
