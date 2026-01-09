using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace KeeperWpf;

public class RatesViewModel : Screen
{
    private readonly KeeperDataModel _keeperDataModel;
    private readonly IWindowManager _windowManager;
    private readonly KeeperDataModelInitializer _keeperDataModelInitializer;

    public OfficialRatesViewModel OfficialRatesViewModel { get; }
    public ExchangeRatesViewModel ExchangeRatesViewModel { get; }
    public GoldRatesViewModel GoldRatesViewModel { get; }
    public RefinancingRatesViewModel RefinancingRatesViewModel { get; }

    private string _officialRatesTabHeader = "loading...";
    public string OfficialRatesTabHeader
    {
        get => _officialRatesTabHeader;
        set
        {
            if (Equals(_officialRatesTabHeader, value)) return;
            _officialRatesTabHeader = value;
            NotifyOfPropertyChange();
        }
    }

    private string _metalRatesTabHeader = "loading...";
    public string MetalRatesTabHeader
    {
        get => _metalRatesTabHeader;
        set
        {
            if (Equals(_metalRatesTabHeader, value)) return;
            _metalRatesTabHeader = value;
            NotifyOfPropertyChange();
        }
    }


    private string _refinancingRatesTabHeader = "loading...";
    public string RefinancingRatesTabHeader
    {
        get => _refinancingRatesTabHeader;
        set
        {
            if (Equals(_refinancingRatesTabHeader, value)) return;
            _refinancingRatesTabHeader = value;
            NotifyOfPropertyChange();
        }
    }




    public RatesViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager,
        KeeperDataModelInitializer keeperDataModelInitializer,
        OfficialRatesViewModel officialRatesViewModel, ExchangeRatesViewModel exchangeRatesViewModel,
        GoldRatesViewModel goldRatesViewModel, RefinancingRatesViewModel refinancingRatesViewModel)
    {
        _keeperDataModel = keeperDataModel;
        _windowManager = windowManager;
        _keeperDataModelInitializer = keeperDataModelInitializer;
        OfficialRatesViewModel = officialRatesViewModel;
        ExchangeRatesViewModel = exchangeRatesViewModel;
        GoldRatesViewModel = goldRatesViewModel;
        RefinancingRatesViewModel = refinancingRatesViewModel;
    }

    public async Task Initialize()
    {
        // эта страница открывается первой
        _keeperDataModelInitializer.GetExchangeRatesFromDb();
        ExchangeRatesViewModel.Initialize();

        // остальные страницы инициализируем в фоне
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(InitializeOtherPages);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    // происходит быстро, можно было и не заморачиваться с названиями на табиках
    private async Task InitializeOtherPages()
    {
        _keeperDataModelInitializer.GetRefinancingRatesFromDb();
        RefinancingRatesViewModel.Initialize();
        RefinancingRatesTabHeader = "Ставка рефинансирования НБ РБ";
        
        _keeperDataModelInitializer.GetOfficialRatesFromDb();
        await OfficialRatesViewModel.Initialize();
        OfficialRatesTabHeader = "Официальные курсы НБ РБ";
        
        _keeperDataModelInitializer.GetMetalRatesFromDb();
        GoldRatesViewModel.Initialize();
        MetalRatesTabHeader = "Золото, закупока минфина";
      
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

    public async Task Close()
    {
        await TryCloseAsync();
    }
}
