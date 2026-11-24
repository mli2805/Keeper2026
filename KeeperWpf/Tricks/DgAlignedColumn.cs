using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace KeeperWpf;

public class DgRightAlignedColumn : DataGridTextColumn
{
    public DgRightAlignedColumn()
    {
        HeaderStyle = new Style(typeof(DataGridColumnHeader))
        {
            Setters =
            {
                new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Right),
                new Setter(TextBlock.PaddingProperty, new Thickness(0, 0, 5, 0)),
            }
        };
        ElementStyle = new Style(typeof(TextBlock))
        {
            Setters =
            {
                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right),
                new Setter(TextBlock.PaddingProperty, new Thickness(0, 0, 5, 0)),
                new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center)
            }
        };
    }
}
public class DgCenterAlignedColumn : DataGridTextColumn
{
    public DgCenterAlignedColumn()
    {
        HeaderStyle = new Style(typeof(DataGridColumnHeader))
        {
            Setters =
            {
                new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center)
            }
        };
        ElementStyle = new Style(typeof(TextBlock))
        {
            Setters =
            {
                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center),
                new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center)
            }
        };
    }
}
