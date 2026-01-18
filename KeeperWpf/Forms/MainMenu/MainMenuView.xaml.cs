namespace KeeperWpf;

/// <summary>
/// Interaction logic for MainMenuView.xaml
/// </summary>
public partial class MainMenuView 
{
    public MainMenuView()
    {
        InitializeComponent();
        Loaded += MainMenuView_Loaded;
    }

    private void MainMenuView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Focus();
    }
}
