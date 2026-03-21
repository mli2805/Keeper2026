using KeeperModels;

namespace KeeperWpf;

public static class TranExtentions
{
    public static TransactionModel Clone(this TransactionModel tran)
    {
        return new TransactionModel
        {
            Id = tran.Id,
            Timestamp = tran.Timestamp,
            Operation = tran.Operation,
            PaymentWay = tran.PaymentWay,
            MyAccount = tran.MyAccount,
            MySecondAccount = tran.MySecondAccount,
            Counterparty = tran.Counterparty,
            Category = tran.Category,
            Amount = tran.Amount,
            Currency = tran.Currency,
            AmountInReturn = tran.AmountInReturn,
            CurrencyInReturn = tran.CurrencyInReturn,
            Tags = [.. tran.Tags],
            Comment = tran.Comment
        };
    }

    public static void CopyInto(this TransactionModel tran, TransactionModel destinationTran)
    {
        destinationTran.Id = tran.Id;
        destinationTran.Timestamp = tran.Timestamp;
        destinationTran.Operation = tran.Operation;
        destinationTran.PaymentWay = tran.PaymentWay;
        destinationTran.MyAccount = tran.MyAccount;
        destinationTran.MySecondAccount = tran.MySecondAccount;
        destinationTran.Counterparty = tran.Counterparty;
        destinationTran.Category = tran.Category;
        destinationTran.Amount = tran.Amount;
        destinationTran.Currency = tran.Currency;
        destinationTran.AmountInReturn = tran.AmountInReturn;
        destinationTran.CurrencyInReturn = tran.CurrencyInReturn;
        destinationTran.Tags = [.. tran.Tags];
        destinationTran.Comment = tran.Comment;
    }
}
