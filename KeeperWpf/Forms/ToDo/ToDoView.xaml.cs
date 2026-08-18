using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeeperWpf;

/// <summary>
/// Interaction logic for ToDoView.xaml
/// </summary>
public partial class ToDoView
{
    public ToDoView()
    {
        InitializeComponent();
    }

    private void TodoTasksGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        var source = e.OriginalSource as DependencyObject;
        var row = FindParent<DataGridRow>(source);

        if (row != null)
        {
            row.IsSelected = true;
            row.Focus();
            return;
        }

        dataGrid.UnselectAll();
        dataGrid.Focus();
    }

    private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T result)
            {
                return result;
            }

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }
}
