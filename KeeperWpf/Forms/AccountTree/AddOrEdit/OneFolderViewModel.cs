using Caliburn.Micro;
using KeeperInfrastructure;
using KeeperModels;
using System.Threading.Tasks;

namespace KeeperWpf;

public class OneFolderViewModel(AccountRepository accountRepository) : Screen
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

    public async Task Save()
    {
        IsSavePressed = true;
        AccountItemInWork.ChildNumber = AccountItemInWork.Parent!.Children.Count + 1;
        if (_isInAddMode)
            await accountRepository.Add(AccountItemInWork);
        else
            await accountRepository.Update(AccountItemInWork);
        await TryCloseAsync();
    }

    public async Task Cancel()
    {
        if (!_isInAddMode)
        {
            AccountItemInWork.Name = _oldName;
        }
        await TryCloseAsync();
    }
}
