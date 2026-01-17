using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public class KeeperDataModel : PropertyChangedBase
{
    public Dictionary<DateTime, OfficialRates> OfficialRates { get; set; } = null!;
    public Dictionary<DateTime, ExchangeRates> ExchangeRates { get; set; } = null!;
    public List<MetalRate> MetalRates { get; set; } = null!;
    public List<RefinancingRate> RefinancingRates { get; set; } = null!;

    public List<TrustAccount> TrustAccounts { get; set; } = null!;
    public List<TrustAssetModel> InvestmentAssets { get; set; } = null!;
    public List<TrustAssetRate> AssetRates { get; set; } = null!;
    public List<TrustTranModel> InvestTranModels { get; set; } = null!;

    public Dictionary<int, TransactionModel> Transactions { get; set; } = null!;


    private ObservableCollection<AccountItemModel> _accountsTree = new ObservableCollection<AccountItemModel>();
    public ObservableCollection<AccountItemModel> AccountsTree
    {
        get => _accountsTree;
        set
        {
            if (Equals(value, _accountsTree)) return;
            _accountsTree = value;
            NotifyOfPropertyChange();
        }
    }

    public Dictionary<int, AccountItemModel> AcMoDict { get; set; } =
        new Dictionary<int, AccountItemModel>();

    public List<DepositOfferModel> DepositOffers { get; set; } = null!;
    public List<CarModel> Cars { get; set; } = null!;
    public List<FuellingModel> FuellingVms { get; set; } = null!;

    public List<CardBalanceMemoModel> CardBalanceMemoModels { get; set; } = null!;

    public List<ButtonCollectionModel> ButtonCollections { get; set; } = null!;
    public List<SalaryChange> SalaryChanges { get; set; } = null!;
    public List<LargeExpenseThreshold> LargeExpenseThresholds { get; set; } = null!;
}