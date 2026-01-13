using System.Windows.Markup;

namespace KeeperWpf;

/// <summary>
/// Interaction logic for OneInvestTranView.xaml
/// </summary>
public partial class OneInvestTranView
{
    public OneInvestTranView()
    {
        InitializeComponent();
        Language = XmlLanguage.GetLanguage("ru-RU");
    }
}
