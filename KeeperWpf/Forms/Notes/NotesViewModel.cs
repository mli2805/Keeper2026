using Caliburn.Micro;

namespace KeeperWpf;


[ExportViewModel]
public class NotesViewModel : Screen
{
    protected override void OnViewLoaded(object view)
    {
        base.OnViewLoaded(view);
        DisplayName = "Notes";
    }
}
