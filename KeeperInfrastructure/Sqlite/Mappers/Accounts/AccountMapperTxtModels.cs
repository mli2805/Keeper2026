using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class AccountMapperTxtModels
{
    public static AccountItemModel ToModel(this Account account)
    {
        return new AccountItemModel(account.Id, account.Name, null)
        {
            ChildNumber = account.ChildNumber,
            Id = account.Id,
            IsFolder = account.IsFolder,
            IsExpanded = account.IsExpanded,
            AssociatedIncomeId = account.AssociatedIncomeId,
            AssociatedExpenseId = account.AssociatedExpenseId,
            AssociatedExternalId = account.AssociatedExternalId,
            AssociatedTagId = account.AssociatedTagId,
            ShortName = account.ShortName,
            ButtonName = account.ButtonName,
            Comment = account.Comment,
        };
    }
}

public static class AccountMapperEfModels
{
    public static AccountEf FromModel(this AccountItemModel accountModel)
    {
        return new AccountEf
        {
            Id = accountModel.Id,
            Name = accountModel.Name,
            ParentId = accountModel.Parent?.Id ?? 0,
            ChildNumber = accountModel.ChildNumber,
            IsFolder = accountModel.IsFolder,
            IsExpanded = accountModel.IsExpanded,
            AssociatedIncomeId = accountModel.AssociatedIncomeId,
            AssociatedExpenseId = accountModel.AssociatedExpenseId,
            AssociatedExternalId = accountModel.AssociatedExternalId,
            AssociatedTagId = accountModel.AssociatedTagId,
            ShortName = accountModel.ShortName,
            ButtonName = accountModel.ButtonName,
            Comment = accountModel.Comment,
        };
    }

    public static void UpdateFromModel(this AccountEf accountEf, AccountItemModel accountModel)
    {
        accountEf.Name = accountModel.Name;
        accountEf.ParentId = accountModel.Parent?.Id ?? 0;
        accountEf.ChildNumber = accountModel.ChildNumber;
        accountEf.IsFolder = accountModel.IsFolder;
        accountEf.IsExpanded = accountModel.IsExpanded;
        accountEf.AssociatedIncomeId = accountModel.AssociatedIncomeId;
        accountEf.AssociatedExpenseId = accountModel.AssociatedExpenseId;
        accountEf.AssociatedExternalId = accountModel.AssociatedExternalId;
        accountEf.AssociatedTagId = accountModel.AssociatedTagId;
        accountEf.ShortName = accountModel.ShortName;
        accountEf.ButtonName = accountModel.ButtonName;
        accountEf.Comment = accountModel.Comment;
    }

    public static BankAccountModel ToModel(this BankAccountEf bankAccountEf)
    {
        return new BankAccountModel
        {
            Id = bankAccountEf.Id,
            BankId = bankAccountEf.BankId,
            DepositOfferId = bankAccountEf.DepositOfferId,
            MainCurrency = bankAccountEf.MainCurrency,
            AgreementNumber = bankAccountEf.AgreementNumber,
            ReplenishDetails = bankAccountEf.ReplenishDetails,
            StartDate = bankAccountEf.StartDate,
            FinishDate = bankAccountEf.FinishDate,
            IsMine = bankAccountEf.IsMine,
        };
    }

    public static BankAccountEf FromModel(this BankAccountModel bankAccountModel)
    {
        return new BankAccountEf
        {
            Id = bankAccountModel.Id,
            BankId = bankAccountModel.BankId,
            DepositOfferId = bankAccountModel.DepositOfferId,
            MainCurrency = bankAccountModel.MainCurrency,
            AgreementNumber = bankAccountModel.AgreementNumber,
            ReplenishDetails = bankAccountModel.ReplenishDetails,
            StartDate = bankAccountModel.StartDate,
            FinishDate = bankAccountModel.FinishDate,
            IsMine = bankAccountModel.IsMine,
        };
    }

    public static void UpdateFromModel(this BankAccountEf bankAccountEf, BankAccountModel bankAccountModel)
    {
        bankAccountEf.BankId = bankAccountModel.BankId;
        bankAccountEf.DepositOfferId = bankAccountModel.DepositOfferId;
        bankAccountEf.MainCurrency = bankAccountModel.MainCurrency;
        bankAccountEf.AgreementNumber = bankAccountModel.AgreementNumber;
        bankAccountEf.ReplenishDetails = bankAccountModel.ReplenishDetails;
        bankAccountEf.StartDate = bankAccountModel.StartDate;
        bankAccountEf.FinishDate = bankAccountModel.FinishDate;
        bankAccountEf.IsMine = bankAccountModel.IsMine;
    }
}
