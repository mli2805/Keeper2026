using System;
using System.Linq;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public static class BankAccountMemoExt
{
    public static bool HasLowBalanceAccounts(this KeeperDataModel dataModel)
    {
        return dataModel.BankAccountMemoModels.Any(m => m.IsLimitExceeded);
    }

    public static decimal GetCurrentBalance(this KeeperDataModel keeperDataModel, AccountItemModel account)
    {
        var accountCalculator =
            new TrafficOfAccountCalculator(keeperDataModel, account,
                new Period(new DateTime(2001, 12, 31), DateTime.Today.GetEndOfDate()));
        var balance = accountCalculator.EvaluateBalance();
        return balance.Currencies.TryGetValue(account.BankAccount!.MainCurrency, out var currency) ? currency : 0;
    }

    public static decimal GetExpenseForCurrentMonth(this KeeperDataModel keeperDataModel, AccountItemModel account)
    {
        var period = DateTime.Today.GetFullMonthForDate();
        var expenseTrans = keeperDataModel.Transactions.Values
            .Where(t => period.Includes(t.Timestamp) 
                        && t.MyAccount == account && t.Operation == OperationType.Расход
                        && t.PaymentWay.IsPaymentByCard(account));
        return expenseTrans.Sum(t => t.Amount);
    }

    private static bool IsPaymentByCard(this PaymentWay paymentWay, AccountItemModel account)
    {
        if (account.Is(884))
            return paymentWay == PaymentWay.КартаТерминал 
                   || paymentWay == PaymentWay.ТелефонТерминал
                   || paymentWay == PaymentWay.ПриложениеПродавца
                   || paymentWay == PaymentWay.ОплатаПоЕрип; // для 123 есть ЕРИП


        return paymentWay == PaymentWay.КартаТерминал 
               || paymentWay == PaymentWay.ТелефонТерминал
               || paymentWay == PaymentWay.ПриложениеПродавца;
    }
}
