using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KeeperWpf;

public static class CarReportDataProvider
{
    // полный раздельный список категорий для отчета по автомобилю
    private static readonly string[] CategoryRussianNames =
        { "покупка-продажа", "погашение кредита",  "государство", "авто топливо",
        "авто ремонт", "ремонт ДТП", "регулярн обслуживание",  "авто прочее" };

    // агрегированные категории для отчета по автомобилю
    private static readonly string[] CategoryRussianNamesBuySell =
         { "покупка-продажа", "погашение кредита" };
    private static readonly string[] CategoryRussianNamesRepair =
         { "авто ремонт", "ремонт ДТП", "регулярн обслуживание" };
    private static readonly string[] CategoryRussianNamesOther =
         { "государство", "авто топливо", "авто прочее" };

    public static CarReportData ExtractCarReportData(this KeeperDataModel dataModel, 
        CarModel car, bool isAggregateCategories)
    {
        var isCurrentCar = dataModel.Cars.Last().Id == car.Id;

        var carReportData = dataModel.ExtractCarData(car.CarAccountId, isAggregateCategories);
        if (isCurrentCar)
        {
            carReportData.Categories[0].Table.Add(new CarReportTableRow()
            {
                Date = DateTime.Today,
                AmountInUsd = car.SupposedSalePrice,
                AmountInCurrency = $"{car.SupposedSalePrice} usd",
                Comment = "предполагаемая продажа",
            });
        }
        carReportData.FinishDate = carReportData.Categories[0].Table.Last().Date;
        return carReportData;
    }

    private static CarReportData ExtractCarData(this KeeperDataModel dataModel,
        int carAccountId, bool isAggregateCategories)
    {
        var result = new CarReportData();
        var carAccount = dataModel.AcMoDict[carAccountId];

        // раздельно по таблице на каждую категорию
        if (!isAggregateCategories)
            for (int i = 0; i < CategoryRussianNames.Length; i++)
            {
                // get Category by name
                var category = (AccountItemModel?)carAccount.Children
                    .FirstOrDefault(c => c.Name.Contains(CategoryRussianNames[i]));
                if (category == null) continue;
                // get rows for Category
                var rows = dataModel.GetTableForOneCategory(category);
                if (rows.Count > 0)
                    result.Categories.Add(new CarReportTable(ToTableName(CategoryRussianNames[i]),  rows));
            }
        else
        {
            // покупка-продажа и погашение кредита в одной таблице
            var dataBuySell = new List<CarReportTableRow>();
            foreach (var categoryName in CategoryRussianNamesBuySell)
            {
                // get Category by name
                var category = (AccountItemModel?)carAccount.Children.FirstOrDefault(c => c.Name.Contains(categoryName));
                if (category == null) continue;
                // get rows for Category
                var rows = dataModel.GetTableForOneCategory(category);
                if (rows.Count > 0)
                    dataBuySell.AddRange(rows);
            }
            var tag = carAccount.Children.Any(c => c.Name.Contains("погашение кредита")) 
                ? "покупка-продажа (включая погашение кредита)" : "покупка-продажа";
            result.Categories.Add(new CarReportTable(tag, dataBuySell.OrderBy(r => r.Date).ToList()));

            // все категории кроме ремонта в отдельных таблицах
            for (int i = 0; i < CategoryRussianNamesOther.Length; i++)
            {
                // get Category by name
                var category = (AccountItemModel?)carAccount.Children
                    .FirstOrDefault(c => c.Name.Contains(CategoryRussianNamesOther[i]));
                if (category == null) continue;
                // get rows for Category
                var rowsOther = dataModel.GetTableForOneCategory(category);
                if (rowsOther.Count > 0)
                    result.Categories.Add(new CarReportTable
                    (ToTableName(CategoryRussianNamesOther[i]), rowsOther));
            }

            // все виды ремонта в одной таблице
            var dataRepair = new List<CarReportTableRow>();
            foreach (var categoryName in CategoryRussianNamesRepair)
            {
                // get Category by name
                var category = (AccountItemModel?)carAccount.Children
                    .FirstOrDefault(c => c.Name.Contains(categoryName));
                if (category == null) continue;
                // get rows for Category
                var rows = dataModel.GetTableForOneCategory(category);
                if (rows.Count > 0)
                    dataRepair.AddRange(rows);
            }
            result.Categories.Add(new CarReportTable("обслуживание и ремонт", 
                dataRepair.OrderBy(r => r.Date).ToList()));

          
        }

        result.StartDate = result.Categories[0].Table[0].Date;
        return result;
    }

    private static List<CarReportTableRow> GetTableForOneCategory(this KeeperDataModel dataModel, AccountItemModel category)
    {
        var rows = new List<CarReportTableRow>();
        foreach (var transaction in dataModel.Transactions.Values
                     .Where(t=>t.Category != null && t.Category.Id == category.Id)
                     .OrderBy(t => t.Timestamp))
        {
            var balanceForTag = transaction.BalanceForCategory();
            if (balanceForTag == null) continue;
            var row = new CarReportTableRow
            {
                Date = transaction.Timestamp,
                AmountInCurrency = balanceForTag.ToString(),
                Comment = transaction.Comment
            };

            if (Regex.IsMatch(transaction.Comment, @"\d{6} км, \w*"))
            {
                var substring = transaction.Comment.Substring(0, 6);
                if (int.TryParse(substring, out int mileage))
                {
                    row.Mileage = mileage;
                    row.Comment = transaction.Comment.Substring(11);
                }
                
            }

            var moneyPair = balanceForTag.Currencies.First();
            row.AmountInUsd = moneyPair.Key == CurrencyCode.USD
                ? moneyPair.Value
                : dataModel.AmountInUsd(transaction.Timestamp, moneyPair.Key, moneyPair.Value);

            rows.Add(row);
        }
        return rows;
    }

    private static string ToTableName(string categoryName)
    {
        if (categoryName.StartsWith("авто"))
            categoryName = categoryName.Substring(5);
        return categoryName;
    }
}