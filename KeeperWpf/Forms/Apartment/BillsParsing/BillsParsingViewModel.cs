using Caliburn.Micro;
using Microsoft.Win32;

namespace KeeperWpf;

[ExportViewModel]
public class BillsParsingViewModel : Screen
{
    private string _directoryPath = @"C:\temp\bills";

    public string DirectoryPath
    {
        get => _directoryPath;
        set
        {
            if (value == _directoryPath) return;
            _directoryPath = value;
            NotifyOfPropertyChange();
        }
    }

    public void SelectDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = DirectoryPath,
            Title = "Выберите каталог"
        };

        if (dialog.ShowDialog() == true)
            DirectoryPath = dialog.FolderName;
    }
}
