using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;

namespace KeeperWpf;

public class OneBankAccountViewModel(KeeperDataModel dataModel, AccountRepository accountRepository) : Screen
{
    private bool _isInAddMode;
    private bool _isCard;

    public AccountItemModel AccountItemInWork { get; set; } = null!;
    public BankAccountModel BankAccountInWork { get; set; } = null!;

    public List<AccountItemModel> Banks { get; set; } = null!;
    public List<string> BankNames { get; set; } = null!;

    private string _selectedBankName = null!;
    public string SelectedBankName
    {
        get => _selectedBankName;
        set
        {
            if (value == _selectedBankName) return;
            _selectedBankName = value;
            BankAccountInWork.BankId = Banks.First(b => b.Name == _selectedBankName).Id;
            DepositOffers = dataModel.DepositOffers.Where(o => o.Bank.Name == _selectedBankName).ToList();
            SelectedDepositOffer = DepositOffers.Last();
            NotifyOfPropertyChange();
        }
    }

    private List<DepositOfferModel> _depositOffers = null!;
    public List<DepositOfferModel> DepositOffers
    {
        get => _depositOffers;
        set
        {
            if (Equals(value, _depositOffers)) return;
            _depositOffers = value;
            NotifyOfPropertyChange();
        }
    }

    private DepositOfferModel _selectedDepositOffer = null!;

    public DepositOfferModel SelectedDepositOffer
    {
        get => _selectedDepositOffer;
        set
        {
            if (Equals(value, _selectedDepositOffer) || value == null) return;
            _selectedDepositOffer = value;
            BankAccountInWork.BankId = _selectedDepositOffer.Bank.Id;
            BankAccountInWork.DepositOfferId =_selectedDepositOffer.Id;
            BankAccountInWork.MainCurrency = _selectedDepositOffer.MainCurrency;
            BankAccountInWork.StartDate = DateTime.Today;
            var finish = _selectedDepositOffer.DepositTerm.AddTo(BankAccountInWork.StartDate);
            BankAccountInWork.FinishDate = _isCard ? finish.GetEndOfMonth() : finish;
            NotifyOfPropertyChange();
        }
    }

    public string ParentName { get; set; } = null!;

    private string _accountName = null!;
    public string AccountName   
    {
        get => _accountName;
        set
        {
            if (value == _accountName) return;
            _accountName = value;
            NotifyOfPropertyChange(() => AccountName);
        }
    }

    public bool IsSavePressed { get; set; }

    public List<PaymentSystem> PaymentSystems { get; set; } = null!;
    public Visibility PayCardSectionVisibility { get; set; }

    public void Initialize(AccountItemModel accountItemInWork, bool isInAddMode, bool isCard)
    {
        _isCard = isCard;
        AccountName = "";
        IsSavePressed = false;
        PayCardSectionVisibility = isCard ? Visibility.Visible : Visibility.Collapsed;
        _isInAddMode = isInAddMode;

        var folder = accountItemInWork.Parent!.Name;

        Banks = dataModel.AcMoDict[220].Children.Select(c=>(AccountItemModel)c).ToList();
        BankNames = Banks.Select(b => b.Name).ToList();

        AccountItemInWork = accountItemInWork;
        BankAccountInWork = AccountItemInWork.BankAccount!.Clone();
        ParentName = accountItemInWork.Parent.Name;
        PaymentSystems = Enum.GetValues(typeof(PaymentSystem)).Cast<PaymentSystem>().ToList();

        if (isInAddMode)
        {
            BankAccountInWork.IsMine = true;
            if (isCard)
                BankAccountInWork.PayCard!.CardHolder = "LEANID MARHOLIN";

            _selectedBankName = BankNames.FirstOrDefault(b=>b == folder) ?? BankNames.First();
            DepositOffers = dataModel.DepositOffers.Where(o => o.Bank.Name == SelectedBankName).ToList();
            SelectedDepositOffer = DepositOffers.Last();
        }
        else
        {
            AccountName = AccountItemInWork.Name;

            var bank = dataModel.AcMoDict[accountItemInWork.BankAccount!.BankId];
            _selectedBankName = bank.Name;
            DepositOffers = dataModel.DepositOffers.Where(o => o.Bank.Name == SelectedBankName).ToList();
            _selectedDepositOffer = DepositOffers.First(o => o.Id == BankAccountInWork.DepositOfferId);
        }
    }

    protected override void OnViewLoaded(object view)
    {
        var cap = _isInAddMode ? "Добавить счет в банке" : "Изменить счет в банке";
        DisplayName = $"{cap} (id = {AccountItemInWork.Id})";
    }

    public void BuildDepoName()
    {
        AccountName = SelectedBankName + " " + SelectedDepositOffer.Title 
                      + " " + BankAccountInWork.FinishDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    public async Task Save()
    {
        IsSavePressed = true;

        AccountItemInWork.BankAccount = BankAccountInWork.Clone();

        AccountItemInWork.Name = string.IsNullOrEmpty(AccountName) ? "Без имени" : AccountName;

        AccountItemInWork.ChildNumber = AccountItemInWork.Parent!.Children.Count + 1;
        if (_isInAddMode)
            await accountRepository.Add(AccountItemInWork);
        else
            await accountRepository.Update(AccountItemInWork);
        await TryCloseAsync();
    }

    public async Task Cancel()
    {
        IsSavePressed = false;
        await TryCloseAsync();
    }

}
