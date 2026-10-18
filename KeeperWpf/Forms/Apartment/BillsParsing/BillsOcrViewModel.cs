using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Win32;

namespace KeeperWpf;

public sealed class BillsOcrViewModel : PropertyChangedBase
{
    public BillsOcrViewModel(KeeperDataModel dataModel, BindableCollection<string>? serviceNames = null)
    {
        Editor = new BillMonthEditorViewModel(dataModel, serviceNames: serviceNames);
    }

    public BillMonthEditorViewModel Editor { get; }
    private readonly BillOcrParser _parser = new();
    private readonly BillParsingCsvStore _csvStore = new();
    private string _directoryPath = @"C:\temp\bills\jpg\ocr";
    private bool _isBusy;
    private int _progressValue;
    private int _progressMaximum = 1;
    private string _status = "Выберите каталог с результатами Tesseract (*.tsv).";

    public BindableCollection<ParsedBillRow> Rows { get; } = new();

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
            _progressValue = value;
            NotifyOfPropertyChange();
        }
    }

    public int ProgressMaximum
    {
        get => _progressMaximum;
        private set
        {
            _progressMaximum = value;
            NotifyOfPropertyChange();
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
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
        var dialog = new OpenFolderDialog { InitialDirectory = DirectoryPath, Title = "Каталог OCR (TSV)" };
        if (dialog.ShowDialog() == true) DirectoryPath = dialog.FolderName;
    }

    public async Task Parse()
    {
        if (!CanParse) return;
        if (!await Editor.SaveChangesAsync())
        {
            Status = Editor.Status;
            return;
        }
        IsBusy = true;
        Rows.Clear();
        ProgressValue = 0;
        Status = "Начат разбор OCR-файлов...";
        try
        {
            ProgressMaximum = Math.Max(1, Directory.EnumerateFiles(DirectoryPath)
                .Count(path => Path.GetExtension(path).Equals(".tsv", StringComparison.OrdinalIgnoreCase)));
            var progress = new Progress<(int Processed, int Total, string FileName)>(value =>
            {
                ProgressValue = value.Processed;
                ProgressMaximum = Math.Max(1, value.Total);
                Status = $"Обработано {value.Processed} из {value.Total}: {value.FileName}";
            });
            var result = await _parser.ParseDirectoryAsync(DirectoryPath, progress);
            Rows.AddRange(result.Rows);
            await Editor.SaveParsedMonthsAsync(result.Rows, result.Footers ?? Array.Empty<BillFooter>());
            Status = $"Разобрано {Rows.Count} строк. Новые месяцы сохранены в {BillMonthJsonStore.DirectoryPath}; существующие JSON не изменены.";
        }
        catch (Exception exception) { Status = $"Ошибка: {exception.Message}"; }
        finally { IsBusy = false; }
    }

    public async Task Save()
    {
        if (!CanSave) return;
        var dialog = new SaveFileDialog
        {
            InitialDirectory = Directory.Exists(DirectoryPath) ? DirectoryPath : null,
            FileName = "bills-ocr-parsed.csv", DefaultExt = ".csv",
            Filter = "CSV (*.csv)|*.csv|Все файлы (*.*)|*.*", Title = "Сохранить результаты OCR"
        };
        if (dialog.ShowDialog() != true) return;
        IsBusy = true;
        try
        {
            await _csvStore.SaveAsync(dialog.FileName, Rows);
            Status = $"Сохранено строк: {Rows.Count}. Файл: {dialog.FileName}";
        }
        catch (Exception exception) { Status = $"Ошибка сохранения: {exception.Message}"; }
        finally { IsBusy = false; }
    }

    public async Task Load()
    {
        if (!CanLoad) return;
        var dialog = new OpenFileDialog
        {
            InitialDirectory = Directory.Exists(DirectoryPath) ? DirectoryPath : null,
            DefaultExt = ".csv", Filter = "CSV (*.csv)|*.csv|Все файлы (*.*)|*.*",
            Title = "Загрузить результаты OCR"
        };
        if (dialog.ShowDialog() != true) return;
        if (!await Editor.SaveChangesAsync())
        {
            Status = Editor.Status;
            return;
        }
        IsBusy = true;
        try
        {
            var rows = await _csvStore.LoadAsync(dialog.FileName);
            Rows.Clear();
            Rows.AddRange(rows);
            Editor.Initialize(rows, Array.Empty<BillFooter>());
            Status = $"Загружено строк: {Rows.Count}. Файл: {dialog.FileName}";
        }
        catch (Exception exception) { Status = $"Ошибка загрузки: {exception.Message}"; }
        finally { IsBusy = false; }
    }
}
