using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Win32;
using System.IO;

namespace KeeperWpf;

[ExportViewModel]
public class BillsParsingViewModel : Screen
{
    private readonly BillPdfParser _parser = new();
    private readonly BillParsingCsvStore _csvStore = new();
    private string _directoryPath = @"C:\temp\bills";
    private bool _isBusy;
    private int _progressValue;
    private int _progressMaximum = 1;
    private string _status = "Выберите каталог с PDF-файлами.";

    public BindableCollection<ParsedBillRow> Rows { get; } = new();
    public BindableCollection<BillParsingDiagnostic> Diagnostics { get; } = new();

    public string DirectoryPath
    {
        get => _directoryPath;
        set
        {
            if (value == _directoryPath) return;
            _directoryPath = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanParse));
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (value == _isBusy) return;
            _isBusy = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanParse));
            NotifyOfPropertyChange(nameof(CanSave));
            NotifyOfPropertyChange(nameof(CanLoad));
            NotifyOfPropertyChange(nameof(CanSelectDirectory));
        }
    }

    public int ProgressValue
    {
        get => _progressValue;
        private set
        {
            if (value == _progressValue) return;
            _progressValue = value;
            NotifyOfPropertyChange();
        }
    }

    public int ProgressMaximum
    {
        get => _progressMaximum;
        private set
        {
            if (value == _progressMaximum) return;
            _progressMaximum = value;
            NotifyOfPropertyChange();
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            if (value == _status) return;
            _status = value;
            NotifyOfPropertyChange();
        }
    }

    public bool CanSelectDirectory => !IsBusy;
    public bool CanParse => !IsBusy && Directory.Exists(DirectoryPath);
    public bool CanSave => !IsBusy && Rows.Count > 0;
    public bool CanLoad => !IsBusy;

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

    public async Task Parse()
    {
        if (!CanParse)
            return;

        IsBusy = true;
        Rows.Clear();
        Diagnostics.Clear();
        ProgressValue = 0;
        ProgressMaximum = Math.Max(1, Directory.EnumerateFiles(DirectoryPath)
            .Count(path => string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase)));
        Status = "Начат разбор PDF-файлов...";

        try
        {
            var progress = new Progress<(int Processed, int Total, string FileName)>(value =>
            {
                ProgressValue = value.Processed;
                ProgressMaximum = Math.Max(1, value.Total);
                Status = $"Обработано {value.Processed} из {value.Total}: {value.FileName}";
            });
            var result = await _parser.ParseDirectoryAsync(DirectoryPath, progress);

            foreach (var row in result.Rows)
                Rows.Add(row);
            foreach (var diagnostic in result.Diagnostics)
                Diagnostics.Add(diagnostic);

            Status = $"Готово: {Rows.Count} строк из {ProgressValue} PDF; ошибок: {Diagnostics.Count}.";
        }
        catch (Exception exception)
        {
            Status = $"Ошибка: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
            NotifyOfPropertyChange(nameof(CanSave));
        }
    }

    public async Task Save()
    {
        if (!CanSave)
            return;

        var dialog = new SaveFileDialog
        {
            InitialDirectory = Directory.Exists(DirectoryPath) ? DirectoryPath : null,
            FileName = "bills-parsed.csv",
            DefaultExt = ".csv",
            Filter = "CSV (*.csv)|*.csv|Все файлы (*.*)|*.*",
            Title = "Сохранить результаты разбора"
        };
        if (dialog.ShowDialog() != true)
            return;

        IsBusy = true;
        try
        {
            await _csvStore.SaveAsync(dialog.FileName, Rows);
            Status = $"Сохранено строк: {Rows.Count}. Файл: {dialog.FileName}";
        }
        catch (Exception exception)
        {
            Status = $"Ошибка сохранения: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task Load()
    {
        if (!CanLoad)
            return;

        var dialog = new OpenFileDialog
        {
            InitialDirectory = Directory.Exists(DirectoryPath) ? DirectoryPath : null,
            DefaultExt = ".csv",
            Filter = "CSV (*.csv)|*.csv|Все файлы (*.*)|*.*",
            Title = "Загрузить результаты разбора"
        };
        if (dialog.ShowDialog() != true)
            return;

        IsBusy = true;
        try
        {
            var rows = await _csvStore.LoadAsync(dialog.FileName);
            Rows.Clear();
            Diagnostics.Clear();
            foreach (var row in rows)
                Rows.Add(row);
            Status = $"Загружено строк: {Rows.Count}. Файл: {dialog.FileName}";
        }
        catch (Exception exception)
        {
            Status = $"Ошибка загрузки: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
            NotifyOfPropertyChange(nameof(CanSave));
        }
    }
}
