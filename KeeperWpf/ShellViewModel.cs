using Caliburn.Micro;
using Microsoft.Extensions.Configuration;

namespace KeeperWpf;

public class ShellViewModel : Screen, IShell
{
    private readonly IConfiguration _configuration;
    private readonly IWindowManager _windowManager;
    private readonly KeeperDataModelInitializer _keeperDataModelInitializer;
    private readonly RatesViewModel _ratesViewModel;

    public ShellViewModel(IConfiguration configuration, IWindowManager windowManager,
        KeeperDataModelInitializer keeperDataModelInitializer,
        RatesViewModel ratesViewModel)
    {
        _configuration = configuration;
        _windowManager = windowManager;
        _keeperDataModelInitializer = keeperDataModelInitializer;
        _ratesViewModel = ratesViewModel;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Keeper 2026";
    }

    protected override async void OnViewReady(object view)
    {
        //
    }

    public async void ShowRatesForm()
    {
        _keeperDataModelInitializer.InitializeExchangeRates();
        await _ratesViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_ratesViewModel);
    }
}