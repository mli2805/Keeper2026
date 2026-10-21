using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace KeeperWpf;

public partial class BillMonthEditorView : UserControl
{
    private bool _updatingActiveCell;

    public BillMonthEditorView()
    {
        InitializeComponent();
        DataContextChanged += (_, args) =>
        {
            if (args.OldValue is BillMonthEditorViewModel oldModel)
            {
                oldModel.CommitPendingEdits = null;
                oldModel.PropertyChanged -= ModelPropertyChanged;
            }
            if (args.NewValue is BillMonthEditorViewModel model)
            {
                model.CommitPendingEdits = CommitPendingEdits;
                model.PropertyChanged += ModelPropertyChanged;
            }
        };
        Loaded += (_, _) =>
        {
            if (DataContext is BillMonthEditorViewModel model)
            {
                model.CommitPendingEdits = CommitPendingEdits;
                model.PropertyChanged -= ModelPropertyChanged;
                model.PropertyChanged += ModelPropertyChanged;
            }
            RestoreNumberSorting();
        };
        Unloaded += (_, _) =>
        {
            if (DataContext is BillMonthEditorViewModel model)
            {
                model.CommitPendingEdits = null;
                model.PropertyChanged -= ModelPropertyChanged;
            }
        };
        DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid))
            .AddValueChanged(MonthRows, (_, _) => Dispatcher.BeginInvoke(DispatcherPriority.DataBind,
                new Action(RestoreNumberSorting)));
    }

    private bool CommitPendingEdits() => MonthRows.CommitEdit(DataGridEditingUnit.Cell, true) &&
        MonthRows.CommitEdit(DataGridEditingUnit.Row, true);

    private void ServiceNamesDropDownOpened(object sender, EventArgs e)
    {
        if (sender is ComboBox comboBox && DataContext is BillMonthEditorViewModel model)
        {
            comboBox.GetBindingExpression(ComboBox.TextProperty)?.UpdateSource();
            model.RefreshServiceNames();
        }
    }

    private void RestoreNumberSorting()
    {
        if (MonthRows.ItemsSource is not ListCollectionView view) return;
        if (view.SortDescriptions.Count != 1 ||
            view.SortDescriptions[0].PropertyName != nameof(BillEditableRow.SortNumber) ||
            view.SortDescriptions[0].Direction != ListSortDirection.Ascending)
        {
            using (view.DeferRefresh())
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(nameof(BillEditableRow.SortNumber), ListSortDirection.Ascending));
            }
        }
        if (!view.LiveSortingProperties.Contains(nameof(BillEditableRow.SortNumber)))
            view.LiveSortingProperties.Add(nameof(BillEditableRow.SortNumber));
        view.IsLiveSorting = true;
    }

    private void MonthRowsSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        if (!_updatingActiveCell && DataContext is BillMonthEditorViewModel model)
            model.SelectedRow = MonthRows.CurrentCell.Item as BillEditableRow;
    }

    private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_updatingActiveCell || e.PropertyName != nameof(BillMonthEditorViewModel.SelectedRow) ||
            sender is not BillMonthEditorViewModel { SelectedRow: { } row } ||
            !MonthRows.Items.Contains(row) || ReferenceEquals(MonthRows.CurrentCell.Item, row)) return;
        _updatingActiveCell = true;
        try
        {
            var column = MonthRows.CurrentCell.Column ?? MonthRows.Columns[0];
            var cell = new DataGridCellInfo(row, column);
            MonthRows.CurrentCell = cell;
            MonthRows.SelectedCells.Clear();
            MonthRows.SelectedCells.Add(cell);
            MonthRows.ScrollIntoView(row, column);
        }
        finally { _updatingActiveCell = false; }
    }
}
