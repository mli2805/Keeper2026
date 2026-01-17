using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KeeperWpf;

public class LoadingProgressViewModel : Screen
{
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly CancellationToken _cancellationToken;
    private readonly IConfiguration _configuration;
    private readonly ToSqlite _toSqlite;


    private string _writingToDbMessage;
    public string WritingToDbMessage
    {
        get => _writingToDbMessage;
        set
        {
            if (Equals(_writingToDbMessage, value)) return;
            _writingToDbMessage = value;
            NotifyOfPropertyChange();
        }
    }


    public LoadingProgressViewModel(IConfiguration configuration, ToSqlite toSqlite)
    {
        _cancellationToken = _cancellationTokenSource.Token;
        _configuration = configuration;
        _toSqlite = toSqlite;
    }

    protected async override void OnViewLoaded(object view)
    {
        DisplayName = "Загрузка...";
        bool s = await Task.Run(Load);

        await TryCloseAsync(s);
    }

    private async Task<bool> Load()
    {
        var keeperDomainModel = await TryLoadAllFromTextFiles();
        if (keeperDomainModel != null)
        {
            WritingToDbMessage = "Запись в базу данных...";
            await _toSqlite.SaveModelToDb(keeperDomainModel!);
            return true;
        }
        else
        {
            return false;
        }
    }

    private async Task<KeeperDomainModel?> TryLoadAllFromTextFiles()
    {
        var backupFolder = Path.Combine(_configuration["DataFolder"] ?? "", "backup");

        KeeperDomainModel? keeperDomainModel;
        try
        {
            keeperDomainModel = await TxtLoader.LoadAllFromTextFiles(backupFolder);
            if (keeperDomainModel is null)
            {
                MessageBox.Show("Ошибка при загрузке данных из текстовых файлов.");
                return null;
            }
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Ошибка при загрузке данных из текстовых файлов: {ex.Message}");
            return null;
        }

        return keeperDomainModel;
    }

    public async Task Cancel()
    {
        _cancellationTokenSource.Cancel();
        await TryCloseAsync(false);
    }
}
