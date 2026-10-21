namespace KeeperWpf;

public static class NickNames
{
    public static readonly int External = 157; // Counterparty
    public static readonly int MyAccounts = 158;
    public static readonly int BankAccounts = 159;
    public static readonly int PayCards = 161;
    public static readonly int Deposits = 166;
    public static readonly int Trusts = 902;
    public static readonly int Debts = 866;

    public static readonly int Closed = 393;

    public static readonly int IncomeCategoriesRoot = 185;
    public static readonly int ExpenseCategoriesRoot = 189;
    public static readonly int TagsRoot = 1014;

    public static readonly int Percents = 208;
    public static readonly int MoneyBack = 701;

    public static bool IsMyAccount(this AccountItemModel account) { return account.Is(MyAccounts); }
    public static bool IsCategory(this AccountItemModel account) // one of Income or Expense
    {
        return account.Is(IncomeCategoriesRoot) || account.Is(ExpenseCategoriesRoot);
    }

    public static bool IsCounterparty(this AccountItemModel account) { return account.Is(External); }
    public static bool IsTag(this AccountItemModel account) { return account.Is(TagsRoot); }


    public static AccountItemModel MineRoot(this KeeperDataModel dataModel)
    {
        return dataModel.AcMoDict[158];
    }

    public static AccountItemModel ExternalRoot(this KeeperDataModel dataModel)
    {
        return dataModel.AcMoDict[157];
    }

    public static AccountItemModel IncomeRoot(this KeeperDataModel dataModel)
    {
        return dataModel.AcMoDict[185];
    }

    public static AccountItemModel ExpensesRoot(this KeeperDataModel dataModel)
    {
        return dataModel.AcMoDict[189];
    }



    public static AccountItemModel MoneyBackCategory(this KeeperDataModel dataModel)
    {
        return dataModel.AcMoDict[701];
    }

    public static AccountItemModel PercentsCategory(this KeeperDataModel dataModel)
    {
        return dataModel.AcMoDict[208];
    }
    public static AccountItemModel CardFeeCategory(this KeeperDataModel dataModel)
    {
        return dataModel.AcMoDict[847];
    }

    public static string GetIconPath(this AccountItemModel accountItemModel)
    {
        if (accountItemModel.IsFolder)
            return "/KeeperWpf;component/Resources/tree16/yellow_folder.png";
        if (accountItemModel.Is(Closed))
            return "/KeeperWpf;component/Resources/tree16/cross.png";
        if (accountItemModel.IsCard)
            return "/KeeperWpf;component/Resources/tree16/paycard4.png";
        if (accountItemModel.IsDeposit)
            return "/KeeperWpf;component/Resources/tree16/deposit7.png";
        if (accountItemModel.Is(Debts))
            return "/KeeperWpf;component/Resources/tree16/hand_point_left.png";
        if (accountItemModel.Is(Trusts))
            return "/KeeperWpf;component/Resources/tree16/trust.png";
        if (accountItemModel.Is(BankAccounts))
            return "/KeeperWpf;component/Resources/tree16/account4.png";
        if (accountItemModel.Is(MyAccounts))
            return "/KeeperWpf;component/Resources/tree16/wallet2.png";
        if (accountItemModel.Is(IncomeCategoriesRoot))
            return "/KeeperWpf;component/Resources/tree16/plus3.png";
        if (accountItemModel.Is(ExpenseCategoriesRoot))
            return "/KeeperWpf;component/Resources/tree16/minus3.png";
        if (accountItemModel.Is(TagsRoot))
            return "/KeeperWpf;component/Resources/tree16/tag.png";

        return "/KeeperWpf;component/Resources/tree16/counterparty.png";
    }
}