using Caliburn.Micro;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeeperWpf;

public class TransactionsViewModel : Screen
{
    private readonly FilterModel _filterModel;
    private readonly FilterViewModel _filterViewModel;
    private readonly TranEditExecutor _tranEditExecutor;
    private readonly TranMoveExecutor _tranMoveExecutor;
    private readonly TranSelectExecutor _tranSelectExecutor;
    private readonly ComboTreesProvider _comboTreesProvider;

    private int _width = 1200;
    public int Width
    {
        get => _width;
        set
        {
            if (value == _width) return;
            _width = value;
            NotifyOfPropertyChange();
        }
    }

    private int _left;
    public int Left
    {
        get { return _left; }
        set
        {
            _left = value;
            _filterViewModel?.PlaceIt(Left, Top, FilterViewWidth);
        }
    }

    public int Top { get; set; }

    public int FilterViewWidth = 225;


    public TranModel Model { get; set; }

    public TransactionsViewModel(TranModel model, FilterModel filterModel, FilterViewModel filterViewModel,
        TranEditExecutor tranEditExecutor, TranMoveExecutor tranMoveExecutor, TranSelectExecutor tranSelectExecutor,
        ComboTreesProvider comboTreesProvider)
    {
        Model = model;
        _filterModel = filterModel;
        _filterViewModel = filterViewModel;
        _tranEditExecutor = tranEditExecutor;
        _tranMoveExecutor = tranMoveExecutor;
        _tranSelectExecutor = tranSelectExecutor;
        _comboTreesProvider = comboTreesProvider;
        Top = 100;
        Left = 400;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Проводки";
    }

    public void Initialize()
    {
        _comboTreesProvider.Initialize();
        _filterModel.Initialize();
        Model.Initialize();
    }

    public async Task ButtonFilter()
    {
        var wm = new WindowManager();

        _filterViewModel.PlaceIt(Left, Top, FilterViewWidth);
        _filterViewModel.PropertyChanged += FilterViewModel_PropertyChanged;
        _filterViewModel.FilterModel.PropertyChanged += FilterModel_PropertyChanged;

        await wm.ShowWindowAsync(_filterViewModel);
    }

    private void FilterModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Model.SelectedTranWrappedForDataGrid != null)
            Model.SelectedTranWrappedForDataGrid.IsSelected = false;

        Model.SortedRows.Refresh();
        Model.SortedRows.MoveCurrentToLast();
        Model.SelectedTranWrappedForDataGrid = (TranWrappedForDataGrid)Model.SortedRows.CurrentItem;
    }

    private void FilterViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsActive" && _filterViewModel.IsActive == false)
        {
            Model.SortedRows.MoveCurrentToLast();
            Model.SelectedTranWrappedForDataGrid = (TranWrappedForDataGrid)Model.SortedRows.CurrentItem;
        }
    }

    public void Calculator()
    {
        System.Diagnostics.Process.Start("calc");
    }

    public async Task OnPreviewKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.C:
                Calculator(); break;
            case Key.F:
                await ButtonFilter();
                break;
            case Key.Space:
                await _tranEditExecutor.EditSelected();
                break;
            case Key.NumPad8:
                if (Keyboard.Modifiers == ModifierKeys.Control)
                    _tranMoveExecutor.MoveSelected(TranMoveExecutor.Destination.Up);
                break;
            case Key.NumPad2:
                if (Keyboard.Modifiers == ModifierKeys.Control)
                    _tranMoveExecutor.MoveSelected(TranMoveExecutor.Destination.Down);
                break;
            case Key.Insert:
                await _tranEditExecutor.AddAfterSelected();
                break;
            case Key.Delete:
                if (Keyboard.Modifiers == ModifierKeys.Control)
                    await _tranEditExecutor.DeleteSelected();
                break;
        }
        e.Handled = true;

    }

    public void GoToDate()
    {
        _tranSelectExecutor.SelectFirstOfDate();
    }

    public void GoToLast()
    {
        _tranSelectExecutor.SelectLast();
    }


    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        await _filterViewModel.TryCloseAsync();
        return await base.CanCloseAsync(cancellationToken);
    }
}