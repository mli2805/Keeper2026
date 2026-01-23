using Caliburn.Micro;
using KeeperModels;

namespace KeeperWpf;

public class OneFolderViewModel : Screen
{
    private bool _isInAddMode;
    private string _oldName = null!;
    public AccountItemModel AccountItemInWork { get; set; } = null!;
    public string ParentFolder { get; set; } = null!;
    public bool IsSavePressed { get; set; }
  
    public void Initialize(AccountItemModel accountInWork, bool isInAddMode)
    {
        IsSavePressed = false;
        AccountItemInWork = accountInWork;
        _isInAddMode = isInAddMode;
        ParentFolder = AccountItemInWork.Parent == null ? "Корневой счет" : AccountItemInWork.Parent.Name;
        _oldName = accountInWork.Name;
    }

    protected override void OnViewLoaded(object view)
    {
        var cap = _isInAddMode ? "Добавить папку" : "Изменить название папки";
        DisplayName = $"{cap} (id = {AccountItemInWork.Id})";
    }

    public async void Save()
    {
        IsSavePressed = true;
        await TryCloseAsync();
    }

    public async void Cancel()
    {
        if (!_isInAddMode)
        {
            AccountItemInWork.Name = _oldName;
        }
        await TryCloseAsync();
    }
}
