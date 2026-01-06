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
    private readonly KeeperDbContext _keeperDbContext;
    private string _message = "Loading data";
    public string Message
    {
        get => _message; set
        {
            if (value == _message) return;
            _message = value;
            NotifyOfPropertyChange();
        }
    }

    public ShellViewModel(IConfiguration configuration, KeeperDbContext keeperDbContext)
    {
        _configuration = configuration;
        _keeperDbContext = keeperDbContext;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Keeper 2026";

    }

    protected override async void OnViewReady(object view)
    {
        base.OnViewReady(view);
        
        var backupFolder = Path.Combine(_configuration["DataFolder"] ?? "", "backup");

        // просто await функции морозит программу
        KeeperModel? model = await Task.Run(() => TxtLoader.LoadAllFromTextFiles(backupFolder));

        Message = model != null ? "Data loaded" : "Failed to load";

        if (model != null)
            await new ToSqlite(_keeperDbContext).SaveModelToDb(model);

        Message = "Ready";
    }
}