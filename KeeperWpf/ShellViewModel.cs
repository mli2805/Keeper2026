using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KeeperWpf;

public class ShellViewModel : Screen, IShell
{
    private readonly IConfiguration _configuration;
    private readonly IWindowManager _windowManager;
    private readonly KeeperDbContext _keeperDbContext;
    private readonly KeeperDataModelInitializer _dataModelInitializer;
    private readonly LoadingProgressViewModel _loadingProgressViewModel;
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

    public AccountTreeViewModel AccountTreeViewModel { get; }


    public ShellViewModel(IConfiguration configuration, IWindowManager windowManager,
        KeeperDbContext keeperDbContext, KeeperDataModelInitializer dataModelInitializer, LoadingProgressViewModel loadingProgressViewModel,
        RatesViewModel ratesViewModel, AccountTreeViewModel accountTreeViewModel)
    {
        _configuration = configuration;
        _windowManager = windowManager;
        _keeperDbContext = keeperDbContext;
        _dataModelInitializer = dataModelInitializer;
        _loadingProgressViewModel = loadingProgressViewModel;
        _ratesViewModel = ratesViewModel;
        AccountTreeViewModel = accountTreeViewModel;
    }

    protected override async void OnViewLoaded(object view)
    {
        DisplayName = "Keeper 2026";
        var success = await LoadAccountsTree();
        if (!success)
        {
            await TryCloseAsync();
            return;
        }
    }

    public async Task<bool> LoadAccountsTree()
    {
        // если БД удалили, она будет создана в AppBootstrapper еще до ShellViewModel
        // GetAccountTreeFromDb вернет false, если в БД нет данных
        if (await _dataModelInitializer.GetAccountTreeFromDb())
        {
            return true;    
        }

        var mb = new MyMessageBoxViewModel(MessageType.Confirmation,
            new List<string>()
            {
                "База данных пуста!", "Загрузить данные из текстовых файлов?"
            });
        var confirmation = await _windowManager.ShowDialogAsync(mb);
        if (confirmation == null || confirmation.Value == false)
        {
            return false;
        }

        var success2 = await _windowManager.ShowDialogAsync(_loadingProgressViewModel);
        if (success2 == null || success2.Value == false)
        {
            return false;
        }

        var success = await _dataModelInitializer.GetAccountTreeFromDb();
        return true;
    }

    public async Task LoadFromTextFiles()
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