using KeeperDomain;

namespace KeeperInfrastructure;

public static class AccountMapperTxtEf
{
    public static AccountEf ToEf(this Account account)
    {
        return new AccountEf
        {
            Id = account.Id,
            Name = account.Name,
            ParentId = account.ParentId,
            ChildNumber = account.ChildNumber,
            IsFolder = account.IsFolder,
            IsExpanded = account.IsExpanded,
            AssociatedIncomeId = account.AssociatedIncomeId,
            AssociatedExpenseId = account.AssociatedExpenseId,
            AssociatedExternalId = account.AssociatedExternalId,
            AssociatedTagId = account.AssociatedTagId,
            ShortName = account.ShortName,
            ButtonName = account.ButtonName,
            Comment = account.Comment
        };
    }

    public static Account FromEf(this AccountEf accountEf)
    {
        return new Account
        {
            Id = accountEf.Id,
            Name = accountEf.Name,
            ParentId = accountEf.ParentId,
            ChildNumber = accountEf.ChildNumber,
            IsFolder = accountEf.IsFolder,
            IsExpanded = accountEf.IsExpanded,
            AssociatedIncomeId = accountEf.AssociatedIncomeId,
            AssociatedExpenseId = accountEf.AssociatedExpenseId,
            AssociatedExternalId = accountEf.AssociatedExternalId,
            AssociatedTagId = accountEf.AssociatedTagId,
            ShortName = accountEf.ShortName,
            ButtonName = accountEf.ButtonName,
            Comment = accountEf.Comment
        };
    }

    public static BankAccountEf ToEf(this BankAccount bankAccount)
    {
        return new BankAccountEf
        {
            Id = bankAccount.Id,
            BankId = bankAccount.BankId,
            DepositOfferId = bankAccount.DepositOfferId,
            MainCurrency = bankAccount.MainCurrency,
            AgreementNumber = bankAccount.AgreementNumber,
            ReplenishDetails = bankAccount.ReplenishDetails,
            IsReplenishStopped = bankAccount.IsReplenishStopped,
            StartDate = bankAccount.StartDate,
            FinishDate = bankAccount.FinishDate,
            IsMine = bankAccount.IsMine
        };
    }

    public static BankAccount FromEf(this BankAccountEf bankAccountEf)
    {
        return new BankAccount
        {
            Id = bankAccountEf.Id,
            BankId = bankAccountEf.BankId,
            DepositOfferId = bankAccountEf.DepositOfferId,
            MainCurrency = bankAccountEf.MainCurrency,
            AgreementNumber = bankAccountEf.AgreementNumber,
            ReplenishDetails = bankAccountEf.ReplenishDetails,
            IsReplenishStopped = bankAccountEf.IsReplenishStopped,
            StartDate = bankAccountEf.StartDate,
            FinishDate = bankAccountEf.FinishDate,
            IsMine = bankAccountEf.IsMine
        };
    }

    public static DepositEf ToEf(this Deposit deposit)
    {
        return new DepositEf
        {
            Id = deposit.Id,
            IsAdditionsBanned = deposit.IsAdditionsBanned
        };
    }

    public static Deposit FromEf(this DepositEf depositEf)
    {
        return new Deposit
        {
            Id = depositEf.Id,
            IsAdditionsBanned = depositEf.IsAdditionsBanned
        };
    }

    public static PayCardEf ToEf(this PayCard payCard)
    {
        return new PayCardEf
        {
            Id = payCard.Id,
            CardNumber = payCard.CardNumber,
            CardHolder = payCard.CardHolder,
            PaymentSystem = payCard.PaymentSystem,
            IsVirtual = payCard.IsVirtual,
            IsPayPass = payCard.IsPayPass
        };
    }

    public static PayCard FromEf(this PayCardEf payCardEf)
    {
        return new PayCard
        {
            Id = payCardEf.Id,
            CardNumber = payCardEf.CardNumber,
            CardHolder = payCardEf.CardHolder,
            PaymentSystem = payCardEf.PaymentSystem,
            IsVirtual = payCardEf.IsVirtual,
            IsPayPass = payCardEf.IsPayPass
        };
    }

    public static void UpdateFromModel(this DepositEf depositEf, Deposit deposit)
    {
        depositEf.IsAdditionsBanned = deposit.IsAdditionsBanned;
    }

    public static void UpdateFromModel(this PayCardEf payCardEf, PayCard payCard)
    {
        payCardEf.CardNumber = payCard.CardNumber;
        payCardEf.CardHolder = payCard.CardHolder;
        payCardEf.PaymentSystem = payCard.PaymentSystem;
        payCardEf.IsVirtual = payCard.IsVirtual;
        payCardEf.IsPayPass = payCard.IsPayPass;
    }
}
