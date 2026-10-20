using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Win32;

namespace KeeperWpf;

[ExportViewModel]
public class HouseholdBillsViewModel : Screen
{
    private readonly KeeperDataModel _dataModel;
    private string _directoryPath;
    private BillMonthEditorViewModel? _editor;

    public HouseholdBillsViewModel(KeeperDataModel dataModel, PathFinder pathFinder)
    {
        _dataModel = dataModel;
        _directoryPath = Path.Combine(pathFinder.GetDataFolder(), "householdbills");
        _editor = new BillMonthEditorViewModel(_dataModel, new BillMonthJsonStore(_directoryPath));
    }

    public string DirectoryPath
    {
        get => _directoryPath;
        private set
        {
            _directoryPath = value;
            NotifyOfPropertyChange();
        }
    }

    public BillMonthEditorViewModel? Editor
    {
        get => _editor;
        private set
        {
            _editor = value;
            NotifyOfPropertyChange();
        }
    }

    public async Task SelectDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = Directory.Exists(DirectoryPath) ? DirectoryPath : null,
            Title = "Выберите каталог с JSON-файлами"
        };

        if (dialog.ShowDialog() != true || Editor is not null && !await Editor.SaveChangesAsync())
            return;

        DirectoryPath = dialog.FolderName;
        Editor = new BillMonthEditorViewModel(_dataModel, new BillMonthJsonStore(DirectoryPath));
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        if (Editor is not null && !await Editor.SaveChangesAsync())
            return false;

        return await base.CanCloseAsync(cancellationToken);
    }
}
