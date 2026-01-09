using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace KeeperWpf;

public class ShellViewModel : Screen, IShell
{
    private readonly IConfiguration _configuration;
    private readonly IWindowManager _windowManager;
    private readonly KeeperDbContext _keeperDbContext;
    private readonly RatesViewModel _ratesViewModel;



    private string _loadMessage;
    public string LoadMessage
    {
        get => _loadMessage;
        set
        {
            if (Equals(_loadMessage, value)) return;
            _loadMessage = value;
            NotifyOfPropertyChange();
        }
    }


    public ShellViewModel(IConfiguration configuration, IWindowManager windowManager,
        KeeperDbContext keeperDbContext, RatesViewModel ratesViewModel)
    {
        _configuration = configuration;
        _windowManager = windowManager;
        _keeperDbContext = keeperDbContext;
        _ratesViewModel = ratesViewModel;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Keeper 2026";
    }

    public async void LoadFromTextFiles()
    {
        var backupFolder = Path.Combine(_configuration["DataFolder"] ?? "", "backup");

        // просто await функции морозит программу
        KeeperModel? model = await Task.Run(() => TxtLoader.LoadAllFromTextFiles(backupFolder));

        LoadMessage = model != null ? "Data loaded" : "Failed to load";

        if (model != null)
            await new ToSqlite(_keeperDbContext).SaveModelToDb(model);

        LoadMessage = "Ready";
    }

    public async void ShowRatesForm()
    {
        await _ratesViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_ratesViewModel);
    }

    public async void ShowCarsForm()
    {
        // 
    }
}