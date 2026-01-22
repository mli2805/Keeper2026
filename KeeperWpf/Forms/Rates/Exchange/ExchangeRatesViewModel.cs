using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;

namespace KeeperWpf;

public class ExchangeRatesViewModel : PropertyChangedBase
{
    private readonly KeeperDataModel _keeperDataModel;
    private readonly ExchangeRatesRepository _exchangeRatesRepository;

    public ObservableCollection<ExchangeRates> Rows { get; set; } = null!;

    public ExchangeRatesViewModel(KeeperDataModel keeperDataModel, ExchangeRatesRepository exchangeRatesRepository)
    {
        _keeperDataModel = keeperDataModel;
        _exchangeRatesRepository = exchangeRatesRepository;
    }

    public void Initialize()
    {
        Rows = new ObservableCollection<ExchangeRates>(_keeperDataModel.ExchangeRates.Values);
    }

    public async void Update()
    {
        var last = Rows.Last();
        _keeperDataModel.ExchangeRates.Remove(last.Date);
        Rows.Remove(last);
        await _exchangeRatesRepository.Delete(last.Date);

        var days = (DateTime.Now - Rows.Last().Date).Days;
        if (days == 0) return;

        var newRates = await ExchangeRatesFetcher.Get("Alfa", days);
        if (newRates == null || newRates.Count == 0) return;

        var middayRates =
            ExchangeRatesFetcher.SelectMiddayRates(newRates.OrderBy(l => l.Date).ToList(), Rows.Last().Date.AddDays(1));

        var lastId = Rows.Last().Id;
        var downloaded = new List<ExchangeRates>();
        foreach (var newRate in middayRates)
        {
            if (!_keeperDataModel.ExchangeRates.ContainsKey(newRate.Date))
            {
                newRate.Id = ++lastId;
                Rows.Add(newRate);
                _keeperDataModel.ExchangeRates.Add(newRate.Date, newRate);
                downloaded.Add(newRate);
            }
        }

        if (downloaded.Count > 0)
        {
            await _exchangeRatesRepository.Add(downloaded);
        }
    }

}
