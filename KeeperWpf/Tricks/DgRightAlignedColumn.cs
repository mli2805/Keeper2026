using System.Windows;
using System.Windows.Controls;

namespace KeeperWpf;

public class DgRightAlignedColumn : DataGridTextColumn
{
    public DgRightAlignedColumn()
    {
        ElementStyle = new Style(typeof(TextBlock))
        {
            Setters =
            {
                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right),
                new Setter(TextBlock.PaddingProperty, new Thickness(0, 0, 5, 0))
            }
        };
    }
}
public class DgCenterAlignedColumn : DataGridTextColumn
{
    public DgCenterAlignedColumn()
    {
        ElementStyle = new Style(typeof(TextBlock))
        {
            Setters =
            {
                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center),
            }
        };
    }
}
