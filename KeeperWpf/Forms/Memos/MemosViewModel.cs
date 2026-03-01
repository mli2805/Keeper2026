using System.Threading.Tasks;
using Caliburn.Micro;

namespace KeeperWpf;

public class MemosViewModel : Screen
{
    public DateMemoSetterViewModel DateMemoSetterViewModel { get; }

    public MemosViewModel(
        DateMemoSetterViewModel dateMemoSetterViewModel)
    {
        DateMemoSetterViewModel = dateMemoSetterViewModel;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Напоминалки";
    }

    public async Task Initialize()
    {
    }

}
