using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KeeperWpf;

[ExportViewModel(ViewModelLifetime.SingleInstance)]
public class LoadingProgressViewModel(PathFinder pathFinder, ToSqlite toSqlite) : Screen
{
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


    private string _writingToDbMessage = null!;
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


    protected async override void OnViewLoaded(object view)
    {
        DisplayName = "Загрузка...";
        bool s = await Task.Run(Load);

        await TryCloseAsync(s);
    }

    private async Task<bool> Load()
    {
        var keeperDomainModel = await TryLoadAllFromTextFiles();
        if (keeperDomainModel == null) return false;

        WritingToDbMessage = "Запись в базу данных...";
        await toSqlite.SaveModelToDb(keeperDomainModel);
        return true;
    }

    private async Task<KeeperDomainModel?> TryLoadAllFromTextFiles()
    {
        var backupFolder = Path.Combine(pathFinder.GetDataFolder(), "backup");

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
        await _cancellationTokenSource.CancelAsync();
        await TryCloseAsync(false);
    }
}
