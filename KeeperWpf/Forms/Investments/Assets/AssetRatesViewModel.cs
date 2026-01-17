using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public class AssetRatesViewModel : Screen
{
    private readonly KeeperDataModel _dataModel;

    public ObservableCollection<TrustAssetRate> Rates { get; set; }
    public TrustAssetRate SelectedRate { get; set; }
    public List<TrustAssetModel> Assets { get; set; }

    public DateTime SelectedDate { get; set; } = DateTime.Today;

    public AssetRatesViewModel(KeeperDataModel dataModel)
    {
        _dataModel = dataModel;
    }

    public void Initialize()
    {
        Assets = _dataModel.InvestmentAssets;
        Rates = new ObservableCollection<TrustAssetRate>(_dataModel.AssetRates);
        SelectedRate = Rates.Last();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Стоимость активов";
    }

    public void DeleteSelected(KeyEventArgs e)
    {
        if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (SelectedRate != null)
                Rates.Remove(SelectedRate);
            e.Handled = true;
        }
        
    }

    public void AddForDate()
    {
        var lastId = Rates.Max(r => r.Id);
        foreach (var asset in Assets.Where(a=>a.Ticker != "CASH"))
        {
            var prev = Rates.FirstOrDefault(r => r.TrustAssetId == asset.Id);
            Rates.Add(new TrustAssetRate()
            {
                Id = ++lastId,
                TrustAssetId = asset.Id, 
                Currency = prev?.Currency ?? CurrencyCode.RUB,
                Unit = prev?.Unit ?? 1,
                Value = 0,
                Date = SelectedDate,
            });
        }
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        _dataModel.AssetRates = Rates.ToList();
        return await base.CanCloseAsync(cancellationToken);
    }
}
