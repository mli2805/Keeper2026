using System.Windows;
using System.Windows.Controls;

namespace KeeperWpf;

/// <summary>
/// Interaction logic for AccNameSelectorView.xaml
/// </summary>
public partial class AccNameSelectorView
{
    public AccNameSelectorView()
    {
        InitializeComponent();
    }
    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var accNameSelectorVm = (AccNameSelectorVm)DataContext;
        var buttonViewModel = (AccNameButtonVm)((Button)sender).DataContext;
        accNameSelectorVm.MyAccName = buttonViewModel.AccName;
    }
}
