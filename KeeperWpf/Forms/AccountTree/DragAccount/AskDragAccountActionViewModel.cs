using Caliburn.Micro;

namespace KeeperWpf;

public enum DragAndDropAction { Before, Inside, After, Cancel }
public class AskDragAccountActionViewModel : Screen
{
    private readonly KeeperDataModel _keeperDataModel;
    public string Account1 { get; set; }
    public string Account2 { get; set; }
    public bool IsInsideEnabled {get; set;}

    public DragAndDropAction Answer { get; set; }

    public AskDragAccountActionViewModel(KeeperDataModel keeperDataModel)
    {
        _keeperDataModel = keeperDataModel;
    }

    public void Init(string account1, string account2)
    {
        Account1 = account1;
        Account2 = account2;
        IsInsideEnabled = !_keeperDataModel.AccountUsedInTransaction(_keeperDataModel.AccountByTitle(account2).Id);
        Answer = DragAndDropAction.Cancel;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Question";
    }

    public async void Before()
    {
        Answer = DragAndDropAction.Before;
        await TryCloseAsync();
    }

    public async void Inside()
    {
        Answer = DragAndDropAction.Inside;
        await TryCloseAsync();
    }

    public async void After()
    {
        Answer = DragAndDropAction.After;
        await TryCloseAsync();
    }

    public async void Cancel()
    {
        Answer = DragAndDropAction.Cancel;
        await TryCloseAsync();
    }

}
