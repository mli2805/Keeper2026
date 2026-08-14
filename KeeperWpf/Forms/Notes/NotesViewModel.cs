using Caliburn.Micro;

namespace KeeperWpf;


[ExportViewModel]
public class NotesViewModel(KeeperDataModel dataModel) : Screen
{
    override protected void OnViewLoaded(object view)
    {
        base.OnViewLoaded(view);
        DisplayName = "Notes";
    }
}
