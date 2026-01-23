using Autofac;
using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeeperWpf;

public class InvestmentAssetsViewModel : Screen
{
    private readonly ILifetimeScope _globalScope;
    private readonly KeeperDataModel _dataModel;
    private readonly IWindowManager _windowManager;

    public ObservableCollection<TrustAssetModel> Assets { get; set; } = null!;
    public TrustAssetModel SelectedAsset { get; set; } = null!;

    public List<AssetType> AssetTypes { get; set; }

    public InvestmentAssetsViewModel(ILifetimeScope globalScope, KeeperDataModel dataModel,
        IWindowManager windowManager)
    {
        _globalScope = globalScope;
        _dataModel = dataModel;
        _windowManager = windowManager;
        AssetTypes = Enum.GetValues(typeof(AssetType)).OfType<AssetType>().ToList();
    }

    public void Initialize()
    {
        Assets = new ObservableCollection<TrustAssetModel>(_dataModel.InvestmentAssets.OrderBy(l => l.Id));
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Инвестиционные активы";
    }

    public async Task ShowAssetAnalysis()
    {
        var vm = _globalScope.Resolve<AssetAnalysisViewModel>();
        vm.Initialize(SelectedAsset);
        await _windowManager.ShowWindowAsync(vm);
    }

    public async Task ShowAssetStatistics()
    {
        var vm = _globalScope.Resolve<AssetStatisticsViewModel>();
        vm.Initialize(SelectedAsset);
        await _windowManager.ShowWindowAsync(vm);
    }

    public async Task OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
        {
            DeleteSelected();
            e.Handled = true;
        }
        else if (e.Key == Key.Insert)
        {
            await AddNewAsset();
            e.Handled = true;

        }
    }

    public async Task AddNewAsset()
    {
        var vm = _globalScope.Resolve<OneAssetViewModel>();
        vm.Initialize(Assets.Last().Id + 1);
        if (await _windowManager.ShowDialogAsync(vm) == true)
            Assets.Add(vm.AssetInWork);
    }

    public async Task EditAsset()
    {
        var vm = _globalScope.Resolve<OneAssetViewModel>();
        vm.Initialize(SelectedAsset);
        if (await _windowManager.ShowDialogAsync(vm) == true)
            SelectedAsset.CopyFrom(vm.AssetInWork);
    }

    public void DeleteSelected()
    {
        if (SelectedAsset != null)
            Assets.Remove(SelectedAsset);
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        foreach (var investmentAssetModel in Assets)
        {
            if (investmentAssetModel.TrustAccount == null)
                investmentAssetModel.TrustAccount =
                    _dataModel.TrustAccounts.First(ta => ta.StockMarket == investmentAssetModel.StockMarket);
        }
        _dataModel.InvestmentAssets = Assets.ToList();
        return await base.CanCloseAsync(cancellationToken);
    }
}
