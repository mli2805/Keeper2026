using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class TransactionsMapper
{
    public static void UpdateEf(this TransactionEf transactionEf, TransactionModel transactionModel)
    {
        transactionEf.Timestamp = transactionModel.Timestamp;
        transactionEf.Operation = transactionModel.Operation;
        transactionEf.PaymentWay = transactionModel.PaymentWay;
        transactionEf.Receipt = transactionModel.Receipt;
        transactionEf.MyAccount = transactionModel.MyAccount.Id;
        transactionEf.MySecondAccount = transactionModel.MySecondAccount?.Id;
        transactionEf.Counterparty = transactionModel.Counterparty?.Id;
        transactionEf.Category = transactionModel.Category?.Id;
        transactionEf.Amount = transactionModel.Amount;
        transactionEf.Currency = transactionModel.Currency;
        transactionEf.AmountInReturn = transactionModel.AmountInReturn != 0 ? transactionModel.AmountInReturn : null;
        transactionEf.CurrencyInReturn = transactionModel.CurrencyInReturn;
        transactionEf.Tags = string.Join("|", transactionModel.Tags.Select(t => t.Id));
        transactionEf.Comment = transactionModel.Comment;
    }

    public static TransactionEf ToEf(this TransactionModel transactionModel)
    {
        return new TransactionEf
        {
            Id = transactionModel.Id,
            Timestamp = transactionModel.Timestamp,
            Operation = transactionModel.Operation,
            PaymentWay = transactionModel.PaymentWay,
            Receipt = transactionModel.Receipt,
            MyAccount = transactionModel.MyAccount.Id,
            MySecondAccount = transactionModel.MySecondAccount?.Id,
            Counterparty = transactionModel.Counterparty?.Id,
            Category = transactionModel.Category?.Id,
            Amount = transactionModel.Amount,
            Currency = transactionModel.Currency,
            AmountInReturn = transactionModel.AmountInReturn != 0 ? transactionModel.AmountInReturn : null,
            CurrencyInReturn = transactionModel.CurrencyInReturn,
            Tags = string.Join("|", transactionModel.Tags.Select(t => t.Id)),
            Comment = transactionModel.Comment
        };
    }

    public static TransactionModel FromEf(this TransactionEf transactionEf, Dictionary<int, AccountItemModel> acMoDict)
    {
        var tagIds = string.IsNullOrWhiteSpace(transactionEf.Tags)
                ? new List<int>()
                : transactionEf.Tags.Split('|').Select(s => int.Parse(s.Trim())).ToList();

        return new TransactionModel
        {
            Id = transactionEf.Id,
            Timestamp = transactionEf.Timestamp,
            Operation = transactionEf.Operation,
            PaymentWay = transactionEf.PaymentWay,
            Receipt = transactionEf.Receipt,
            MyAccount = acMoDict[transactionEf.MyAccount],
            MySecondAccount = transactionEf.MySecondAccount == null ? null : acMoDict[transactionEf.MySecondAccount!.Value],
            Counterparty = transactionEf.Counterparty == null ? null : acMoDict[transactionEf.Counterparty!.Value],
            Category = transactionEf.Category == null ? null : acMoDict[transactionEf.Category!.Value],
            Amount = transactionEf.Amount,
            AmountInReturn = transactionEf.AmountInReturn ?? 0,
            Currency = transactionEf.Currency,
            CurrencyInReturn = transactionEf.CurrencyInReturn,
            Tags = tagIds.Select(i => acMoDict[i]).ToList(),
            Comment = transactionEf.Comment
        };
    }

    public static TransactionEf ToEf(this Transaction transaction)
    {
        return new TransactionEf
        {
            Id = transaction.Id,
            Timestamp = transaction.Timestamp,
            Operation = transaction.Operation,
            PaymentWay = transaction.PaymentWay,
            Receipt = transaction.Receipt,
            MyAccount = transaction.MyAccount,
            MySecondAccount = transaction.MySecondAccount != -1 ? transaction.MySecondAccount : null,
            Counterparty = transaction.Counterparty != -1 ? transaction.Counterparty : null,
            Category = transaction.Category != -1 ? transaction.Category : null,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            AmountInReturn = transaction.AmountInReturn != 0 ? transaction.AmountInReturn : null,
            CurrencyInReturn = transaction.CurrencyInReturn,
            Tags = transaction.Tags,
            Comment = transaction.Comment
        };
    }

    public static Transaction FromEf(this TransactionEf transactionEf)
    {
        return new Transaction
        {
            Id = transactionEf.Id,
            Timestamp = transactionEf.Timestamp,
            Operation = transactionEf.Operation,
            PaymentWay = transactionEf.PaymentWay,
            Receipt = transactionEf.Receipt,
            MyAccount = transactionEf.MyAccount,
            MySecondAccount = transactionEf.MySecondAccount ?? -1,
            Counterparty = transactionEf.Counterparty ?? -1,
            Category = transactionEf.Category ?? -1,
            Amount = transactionEf.Amount,
            AmountInReturn = transactionEf.AmountInReturn ?? 0,
            Currency = transactionEf.Currency,
            CurrencyInReturn = transactionEf.CurrencyInReturn,
            Tags = transactionEf.Tags ?? "",
            Comment = transactionEf.Comment
        };
    }
}
