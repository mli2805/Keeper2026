using Caliburn.Micro;
using System.Threading.Tasks;

namespace KeeperWpf;

public class AskReceiptDeletionViewModel : Screen
{
    public int Result { get; set; }
    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Выберите";
        Result = 0;
    }

    public async Task WholeReceipt()
    {
        Result = 99;
        await TryCloseAsync();
    }

    public async Task OneTransaction()
    {
        Result = 1;
        await TryCloseAsync();
    }

    public async Task Cancel()
    {
        Result = 0;
        await TryCloseAsync();
    }
}
