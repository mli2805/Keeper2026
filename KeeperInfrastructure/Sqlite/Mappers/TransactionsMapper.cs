using KeeperDomain;

namespace KeeperInfrastructure;

public static class TransactionsMapper
{
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

    public static FuellingEf ToEf(this Fuelling fuelling)
    {
        return new FuellingEf
        {
            Id = fuelling.Id,
            TransactionId = fuelling.TransactionId,
            CarAccountId = fuelling.CarAccountId,
            Volume = fuelling.Volume,
            FuelType = fuelling.FuelType,
        };
    }

    public static Fuelling FromEf(this FuellingEf fuellingEf)
    {
        return new Fuelling
        {
            Id = fuellingEf.Id,
            TransactionId = fuellingEf.TransactionId,
            CarAccountId = fuellingEf.CarAccountId,
            Volume = fuellingEf.Volume,
            FuelType = fuellingEf.FuelType,
        };
    }
}
