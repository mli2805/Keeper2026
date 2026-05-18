using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace KeeperWpf;

/// <summary>
/// Interaction logic for GoldRatesView.xaml
/// </summary>
public partial class GoldRatesView
{
    public GoldRatesView()
    {
        InitializeComponent();
    }

    private void HandleLinkClick(object sender, RoutedEventArgs e)
    {
        var hl = (Hyperlink)sender;

        Process.Start(new ProcessStartInfo
        {
            FileName = hl.NavigateUri.AbsoluteUri,
            UseShellExecute = true
        });

        e.Handled = true;
    }
}
