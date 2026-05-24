using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace KeeperWpf;

[ExportViewModel]
public class RatesViewModel : Screen
{
    private readonly KeeperDataModel _keeperDataModel;
    private readonly IWindowManager _windowManager;

    public OfficialRatesViewModel OfficialRatesViewModel { get; }
    public ExchangeRatesViewModel ExchangeRatesViewModel { get; }
    public GoldRatesViewModel GoldRatesViewModel { get; }
    public RefinancingRatesViewModel RefinancingRatesViewModel { get; }

   
    public RatesViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager,
        OfficialRatesViewModel officialRatesViewModel, ExchangeRatesViewModel exchangeRatesViewModel,
        GoldRatesViewModel goldRatesViewModel, RefinancingRatesViewModel refinancingRatesViewModel)
    {
        _keeperDataModel = keeperDataModel;
        _windowManager = windowManager;
        OfficialRatesViewModel = officialRatesViewModel;
        ExchangeRatesViewModel = exchangeRatesViewModel;
        GoldRatesViewModel = goldRatesViewModel;
        RefinancingRatesViewModel = refinancingRatesViewModel;
    }

    public async Task Initialize()
    {
        ExchangeRatesViewModel.Initialize();

        // инициализация официальных курсов происходит долго и её запустил сразу после вычитки из DB в KeeperDataModelInitializer
        // Task.Run(OfficialRatesViewModel.Initialize();

        GoldRatesViewModel.Initialize();
        RefinancingRatesViewModel.Initialize();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Курсы валют";
    }

    #region Charts

    private const string OxyplotKey = "A - Reset zoom  ;  Ctrl+RightMouse - Rectangle Zoom";
    public async Task LongTermChart()
    {
        var longTermChartViewModel = new LongTermChartViewModel();
        longTermChartViewModel.Initalize(OxyplotKey, OfficialRatesViewModel.Rows.ToList(), _keeperDataModel);
        await _windowManager.ShowWindowAsync(longTermChartViewModel);
    }

    public async Task UsdFourYearsChart()
    {
        var usdAnnualDiagramViewModel = new UsdAnnualDiagramViewModel();
        usdAnnualDiagramViewModel.Initialize(OxyplotKey, _keeperDataModel);
        await _windowManager.ShowWindowAsync(usdAnnualDiagramViewModel);
    }

    public async Task UsdFiveYearsChart()
    {
        var vm = new UsdFiveInOneChartViewModel();
        vm.Initialize(OxyplotKey, _keeperDataModel);
        await _windowManager.ShowWindowAsync(vm);
    }

    public async Task RusBelChart()
    {
        var vm = new RusBelChartViewModel();
        vm.Initialize(OxyplotKey, _keeperDataModel);
        await _windowManager.ShowWindowAsync(vm);
    }

    public async Task BasketChart()
    {
        var basketDiagramViewModel = new BasketDiagramViewModel();
        basketDiagramViewModel.Initalize(OxyplotKey, OfficialRatesViewModel.Rows.ToList());
        await _windowManager.ShowWindowAsync(basketDiagramViewModel);
    }

    public async Task ProbabilityChart()
    {
        var vm = new NbUsdProbabilitiesViewModel();
        vm.Initialize(OxyplotKey, OfficialRatesViewModel.Rows.ToList());
        await _windowManager.ShowWindowAsync(vm);
    }

    public async Task MonthlyChart()
    {
        var monthlyChartViewModel = new MonthlyChartViewModel();
        monthlyChartViewModel.Initialize(OxyplotKey, _keeperDataModel);
        await _windowManager.ShowWindowAsync(monthlyChartViewModel);
    }

    #endregion

    public async Task CloseView()
    {
        await TryCloseAsync();
    }
}
