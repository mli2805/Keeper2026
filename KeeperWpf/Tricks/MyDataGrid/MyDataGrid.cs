using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace KeeperWpf;

public class MyDataGrid : DataGrid
{
    private static readonly Brush SelectedRowBrush = 
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6E6FF"));

    private static readonly Style CellStyleDefault = CreateCellStyle();
    private static readonly Style RowStyleDefault = CreateRowStyle();

    static MyDataGrid()
    {
        HorizontalGridLinesBrushProperty.OverrideMetadata(typeof(MyDataGrid),
            new FrameworkPropertyMetadata(Brushes.LightGray));
        VerticalGridLinesBrushProperty.OverrideMetadata(typeof(MyDataGrid),
            new FrameworkPropertyMetadata(Brushes.LavenderBlush));
    }

    public MyDataGrid()
    {
        CellStyle = CellStyleDefault;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is DataGridRow row && row.ReadLocalValue(StyleProperty) == DependencyProperty.UnsetValue)
            row.Style = RowStyleDefault;
    }

    private static Style CreateCellStyle()
    {
        var style = new Style(typeof(DataGridCell));
        var trigger = new Trigger { Property = DataGridCell.IsSelectedProperty, Value = true };
        trigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
        trigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.Black));
        trigger.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        style.Triggers.Add(trigger);
        return style;
    }

    private static Style CreateRowStyle()
    {
        var style = new Style(typeof(DataGridRow));
        style.Setters.Add(new Setter(DataGridRow.IsSelectedProperty, new Binding("IsSelected") { Mode = BindingMode.TwoWay }));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.Black));
        var trigger = new Trigger { Property = DataGridRow.IsSelectedProperty, Value = true };
        trigger.Setters.Add(new Setter(Control.BackgroundProperty, SelectedRowBrush));
        style.Triggers.Add(trigger);
        return style;
    }
}
