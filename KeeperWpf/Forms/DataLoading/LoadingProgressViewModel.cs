using Caliburn.Micro;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KeeperWpf;

[ExportViewModel(ViewModelLifetime.SingleInstance)]
public class LoadingProgressViewModel(PathFinder pathFinder, KeeperDomainModelToSqliteSaver toSqlite) : Screen
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
        var keeperDomainModel = await TryLoadAllFromBackupFiles();
        if (keeperDomainModel == null) return false;

        WritingToDbMessage = "Запись в базу данных...";
        await toSqlite.SaveModelToDb(keeperDomainModel);
        return true;
    }

    private async Task<KeeperDomainModel?> TryLoadAllFromBackupFiles()
    {
        var backupFolder = pathFinder.GetJsonBackupFolder();

        KeeperDomainModel? keeperDomainModel;
        try
        {
            keeperDomainModel = await KeeperDomainModelFromJsonLoader.LoadAllFromJsonFiles(backupFolder);
            if (keeperDomainModel is null)
            {
                MessageBox.Show("Ошибка при загрузке данных из JSON файлов.");
                return null;
            }
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Ошибка при загрузке данных из JSON файлов: {ex.Message}");
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
