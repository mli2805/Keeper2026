using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace KeeperWpf;

public class DgLeftAlignedWrappedColumn : DataGridTextColumn
{
    public DgLeftAlignedWrappedColumn()
    {
        HeaderStyle = new Style(typeof(DataGridColumnHeader))
        {
            Setters =
            {
                new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
                new Setter(TextBlock.PaddingProperty, new Thickness(5, 0, 0, 0)),
            }
        };
        ElementStyle = new Style(typeof(TextBlock))
        {
            Setters =
            {
                new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap),
                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Left),
                new Setter(TextBlock.PaddingProperty, new Thickness(5, 0, 0, 0)),
                new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center)
            }
        };
    }
}