using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace KeeperWpf;

public static class GasReadingsImporter
{
    private static readonly Regex NumberRegex = new(
        @"\d+(?:[.,]\d+)?",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static IReadOnlyList<UtilityMetersModel> Import(
        IEnumerable<TransactionModel> transactions,
        IEnumerable<UtilityMetersModel> utilityMeters)
    {
        var emptyRowsByMonth = utilityMeters
            .Where(IsGasEmpty)
            .ToDictionary(row => (row.ReadingsDate.Year, row.ReadingsDate.Month));
        var updatedRows = new List<UtilityMetersModel>();

        foreach (var transaction in transactions
                     .Where(IsGasTransaction)
                     .OrderBy(transaction => transaction.Timestamp))
        {
            var month = (transaction.Timestamp.Year, transaction.Timestamp.Month);
            if (!TryParse(transaction.Comment, out var readings)
                || !emptyRowsByMonth.Remove(month, out var row))
            {
                continue;
            }

            row.ReadingsDate = transaction.Timestamp;
            row.Gas.To = readings.To;
            row.Gas.From = readings.From;
            row.Gas.Volume = readings.Volume;
            updatedRows.Add(row);
        }

        return updatedRows;
    }

    public static IReadOnlyList<UtilityMetersModel> CalculatePreviousReadings(
        IEnumerable<UtilityMetersModel> utilityMeters)
    {
        var rows = utilityMeters
            .OrderBy(row => row.ReadingsDate)
            .ToArray();
        var updatedRows = new List<UtilityMetersModel>();

        for (var anchorIndex = 0; anchorIndex < rows.Length; anchorIndex++)
        {
            var anchor = rows[anchorIndex];
            var previousReadingIsMissing = anchorIndex > 0
                && rows[anchorIndex - 1].Gas.Volume != 0
                && rows[anchorIndex - 1].Gas.To == 0;
            if (anchor.Gas.To == 0
                || anchor.Gas.Volume == 0
                || (anchor.Gas.From != 0 && !previousReadingIsMissing))
            {
                continue;
            }

            var currentTo = anchor.Gas.To;
            for (var index = anchorIndex; index >= 0; index--)
            {
                var row = rows[index];
                if (row.Gas.Volume == 0
                    || (index < anchorIndex && row.Gas.To != 0))
                {
                    break;
                }

                row.Gas.To = currentTo;
                row.Gas.From = currentTo - row.Gas.Volume;
                currentTo = row.Gas.From;
                updatedRows.Add(row);
            }
        }

        return updatedRows;
    }

    private static bool IsGasTransaction(TransactionModel transaction) =>
        string.Equals(transaction.Category?.Name, "Газ", StringComparison.OrdinalIgnoreCase);

    private static bool IsGasEmpty(UtilityMetersModel row) =>
        row.Gas.From == 0
        && row.Gas.To == 0
        && row.Gas.Volume == 0;

    private static bool TryParse(string comment, out GasReadings readings)
    {
        readings = default;
        var numbers = NumberRegex.Matches(comment)
            .Select(match => match.Value)
            .Select(value => TryParseNumber(value, out var number) ? number : (double?)null)
            .ToArray();

        if (numbers.Any(number => number is null))
        {
            return false;
        }

        if (numbers.Length == 1)
        {
            readings = new GasReadings(0, 0, numbers[0]!.Value);
            return true;
        }

        if (numbers.Length != 3)
            return false;

        var values = numbers.Select(number => number!.Value).ToArray();
        var to = values[0];
        var from = values[1];
        var volume = values[2];
        if (Math.Abs(to - from - volume) > 0.000001)
            return false;

        readings = new GasReadings(to, from, volume);
        return true;
    }

    private static bool TryParseNumber(string value, out double result) =>
        double.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private readonly record struct GasReadings(double To, double From, double Volume);
}
