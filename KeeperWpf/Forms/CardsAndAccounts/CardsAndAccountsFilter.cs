using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace KeeperWpf;

public class CardsAndAccountsFilter : PropertyChangedBase
{
    #region Account or Card

    private bool _accountsAndCards;
    private bool _accountsOnly;
    private bool _cardsOnly = true;

    public bool AccountsAndCards    
    {
        get => _accountsAndCards;
        set
        {
            _accountsAndCards = value;
            NotifyOfPropertyChange();
        }
    }

    public bool AccountsOnly    
    {
        get => _accountsOnly;
        set
        {
            _accountsOnly = value;
            NotifyOfPropertyChange();
        }
    }

    public bool CardsOnly
    {
        get => _cardsOnly;
        set
        {
            _cardsOnly = value;
            NotifyOfPropertyChange();
        }
    }

    #endregion

    #region owner
    private bool _ownerAll = true;
    private bool _ownerMine;
    private bool _ownerYuliya;

    public bool OwnerAll
    {
        get => _ownerAll;
        set
        {
            _ownerAll = value;
            NotifyOfPropertyChange();
        }
    }

    public bool OwnerMine
    {
        get => _ownerMine;
        set
        {
            _ownerMine = value;
            NotifyOfPropertyChange();
        }
    }

    public bool OwnerYuliya
    {
        get => _ownerYuliya;
        set
        {
            _ownerYuliya = value;
             NotifyOfPropertyChange();
        }
    }
    #endregion

    public List<string> Currencies { get; set; } = new List<string>() { "все", "byn", "usd", "euro", "rur", "cny" };

    private string _selectedCurrency = null!;
    public string SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (value == _selectedCurrency) return;
            _selectedCurrency = value;
            NotifyOfPropertyChange();
        }
    }

    private double _balance;

    public double Balance
    {
        get => _balance;
        set
        {
            if (value.Equals(_balance)) return;
            _balance = value;
            NotifyOfPropertyChange();
        }
    }

    public CardsAndAccountsFilter()
    {
        SelectedCurrency = Currencies.First();
    }

    public bool Allow(CardOrAccountVm cardOrAccountVm)
    {
        if (_cardsOnly && cardOrAccountVm.PaymentSystem == PaymentSystem.CurrentAccount) return false;
        if (_accountsOnly && cardOrAccountVm.PaymentSystem != PaymentSystem.CurrentAccount) return false;

        if (_ownerYuliya && cardOrAccountVm.IsMine) return false;
        if (_ownerMine && !cardOrAccountVm.IsMine) return false;

        if (_selectedCurrency != "все" && _selectedCurrency != cardOrAccountVm.MainCurrency.ToString().ToLower()) return false;

        if ((double)cardOrAccountVm.Amount < Balance) return false;

        return true;
    }

    public void ClearAll()
    {
        Balance = 0;
        SelectedCurrency = "все";
        OwnerAll = true;
        AccountsAndCards = true;
    }

    public void ShowAllByn5()
    {
        Balance = 5.01;
        SelectedCurrency = "byn";
        OwnerAll = true;
        AccountsAndCards = true;
    }
    
    public void ShowMineByn5()
    {
        Balance = 5.01;
        SelectedCurrency = "byn";
        OwnerMine = true;
        AccountsAndCards = true;
    }
}
