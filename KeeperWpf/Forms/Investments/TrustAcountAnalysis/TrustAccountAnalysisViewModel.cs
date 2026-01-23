using System;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace KeeperWpf;

public class TrustAccountAnalysisViewModel : Screen
{
    private readonly KeeperDataModel _dataModel;
    private TrustAccount _trustAccount = null!;

    public AssetsTableViewModel OpenAssetsViewModel { get; set; } = new AssetsTableViewModel();
    public AssetsTableViewModel ClosedAssetsViewModel { get; set; } = new AssetsTableViewModel();

    public string TransferredToTrustStr { get; set; } = null!;
    public string RealExternalsStr { get; set; } = null!;
    public string AllCurrentActivesStr { get; set; } = null!;   
    public string FinResultStr { get; set; } = null!;
    public string FinPercentStr { get; set; } = null!;


    public string CashStr { get; set; } = null!;
    public string OperationsFeeStr { get; set; } = null!;
    public string BaseFeeStr { get; set; } = null!;
    public string NotPaidFeesStr { get; set; } = null!;
    public string AllPaidFeesStr { get; set; } = null!;

    public TrustAccountAnalysisViewModel(KeeperDataModel dataModel)
    {
        _dataModel = dataModel;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"{_trustAccount.Title}";
    }

    public void Initialize(TrustAccount trustAccount, DateTime date)
    {
        _trustAccount = trustAccount;
        var bal = _dataModel.GetBalancesOfEachAssetOfAccount(trustAccount, date);

        OpenAssetsViewModel.Initialize(bal.Assets.Where(a => a.Quantity > 0).ToList(), _trustAccount);
        if (bal.Assets.Any(a => a.Quantity == 0))
            ClosedAssetsViewModel.Initialize(bal.Assets.Where(a => a.Quantity == 0).ToList(), _trustAccount);

        EvaluateTotals(bal);
    }

    private void EvaluateTotals(TrustAccountBalanceOnDate bal)
    {
        TransferredToTrustStr = $"{bal.TopUp:N} - {bal.Withdraw:N} = {bal.TopUp - bal.Withdraw:N}";
        RealExternalsStr = $"{bal.TopUp:N} + {bal.AllPaidFees:N} - {bal.Withdraw:N} = {bal.Externals:N}";
        AllCurrentActivesStr = $"{bal.AllCurrentActives:N} + {bal.Cash:N} = {bal.AllCurrentActives + bal.Cash:N}";
        FinResultStr = $"{bal.AllCurrentActives + bal.Cash:N} - {bal.Externals:N} = {bal.FinResult:N}";
        FinPercentStr = $" {bal.FinResult / bal.Externals * 100:N}%";

        CashStr = $"{bal.Cash:N}";
        AllPaidFeesStr = $"{bal.OperationsFee:N} + {bal.BaseFee:N} = {bal.AllPaidFees:N}";
        OperationsFeeStr = $"{bal.OperationsFee:N}";
        BaseFeeStr = $"{bal.BaseFee:N}";
        NotPaidFeesStr = $"{bal.NotPaidFees:N}";
    }
}
