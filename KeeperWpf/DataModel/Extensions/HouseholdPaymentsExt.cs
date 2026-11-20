using System.Collections.Generic;
using System.Linq;

namespace KeeperWpf;

public static class HouseholdPaymentsExt
{
    public static List<TransactionModel> GetPreviousPayments(this KeeperDataModel dataModel)
    {
        var result = new List<TransactionModel>();
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 285)); // Погашение кредита ЖСК
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 278)); // По жировке
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 279)); // Дом телефон
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 280)); // Электричество
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 281)); // Газ
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 282)); // Интернет
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 599)); // Дача-Вывоз мусора
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 364)); // Дача-Электричество
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Category != null && t.Category.Id == 519)); // Дача-Zala
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Counterparty != null && t.Counterparty.Id == 270 && t.Comment == "мой")); // мой Велком
        result.Add(dataModel.Transactions.Values
            .Last(t => t.Counterparty != null && t.Counterparty.Id == 270 && t.Comment == "Юля")); // Юля Велком

        return result;
    }
}
