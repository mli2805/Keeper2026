using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace KeeperWpf;

public sealed class BillPdfParser
{
    private const double RowTolerance = 8;
    private static readonly Regex DatePattern = new(@"^\d{2}\.\d{2}\.\d{4}$", RegexOptions.Compiled);
    private static readonly Regex NumberPattern = new(@"^-?\d+(?:[.,]\d+)?$", RegexOptions.Compiled);

    public async Task<BillParsingResult> ParseDirectoryAsync(
        string directoryPath,
        IProgress<(int Processed, int Total, string FileName)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var rows = new List<ParsedBillRow>();
            var diagnostics = new List<BillParsingDiagnostic>();
            var files = Directory.EnumerateFiles(directoryPath)
                .Where(path => string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase))
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            for (var index = 0; index < files.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = files[index];
                var fileName = Path.GetFileName(file);

                try
                {
                    if (!TryGetPeriod(fileName, out var period))
                    {
                        diagnostics.Add(new(fileName, "Имя файла не начинается с периода в формате yyyyMM."));
                        continue;
                    }

                    var fileRows = ParseFile(file, period);
                    if (fileRows.Count == 0)
                        diagnostics.Add(new(fileName, "Не удалось найти строки таблицы начислений."));
                    else
                        rows.AddRange(fileRows);
                }
                catch (Exception exception)
                {
                    diagnostics.Add(new(fileName, exception.Message));
                }
                finally
                {
                    progress?.Report((index + 1, files.Length, fileName));
                }
            }

            return new BillParsingResult(rows, diagnostics);
        }, cancellationToken);
    }

    private static List<ParsedBillRow> ParseFile(string filePath, DateTime period)
    {
        using var document = PdfDocument.Open(filePath);
        var rows = new List<ParsedBillRow>();

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().Select(PositionedWord.From).ToList();
            if (!TryDetectColumns(words, out var columns))
                continue;

            rows.AddRange(ParseRows(words, columns, period, Path.GetFileName(filePath)));
        }

        return rows;
    }

    private static bool TryDetectColumns(IReadOnlyList<PositionedWord> words, out TableColumns columns)
    {
        var tariffHeader = words
            .Where(word => string.Equals(word.Text.Trim('*', '/'), "Тариф",
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(word => word.Left)
            .FirstOrDefault();
        var chargedHeader = words
            .Where(word => string.Equals(word.Text.Trim('*', '/'), "Начислено",
                               StringComparison.OrdinalIgnoreCase) &&
                           Math.Abs(word.CenterY - tariffHeader!.CenterY) <= 10)
            .OrderBy(word => word.Left)
            .FirstOrDefault();

        if (tariffHeader is null || chargedHeader is null)
        {
            columns = default;
            return false;
        }

        var headerY = chargedHeader.CenterY;
        var headerWords = words
            .Where(word => Math.Abs(word.CenterY - headerY) <= 14)
            .ToList();

        var unitHeader = headerWords.FirstOrDefault(word =>
            word.Text.StartsWith("Ед", StringComparison.OrdinalIgnoreCase));
        var volumeHeader = headerWords.FirstOrDefault(word =>
            word.Text.StartsWith("Объ", StringComparison.OrdinalIgnoreCase) ||
            word.Text.StartsWith("Кол-во", StringComparison.OrdinalIgnoreCase));
        var nextColumnHeader = headerWords
            .Where(word => word.Left > chargedHeader.Left &&
                           (word.Text.StartsWith("Сумма", StringComparison.OrdinalIgnoreCase) ||
                            word.Text.StartsWith("Итого", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(word => word.Left)
            .FirstOrDefault();

        if (unitHeader is null || volumeHeader is null)
        {
            columns = default;
            return false;
        }

        columns = new TableColumns(
            headerY,
            unitHeader.Left - 8,
            volumeHeader.Left - 5,
            tariffHeader.Left,
            chargedHeader.Left,
            nextColumnHeader?.Left ?? chargedHeader.Left + 55);
        return true;
    }

    private static IEnumerable<ParsedBillRow> ParseRows(
        IReadOnlyList<PositionedWord> words,
        TableColumns columns,
        DateTime period,
        string sourceFile)
    {
        var tableWords = words
            .Where(word => word.CenterY < columns.HeaderY - 5)
            .ToList();
        var rowAnchors = tableWords
            .Where(word => word.Left < 30 && int.TryParse(word.Text, out var number) && number is > 0 and < 100)
            .OrderByDescending(word => word.CenterY)
            .ToList();

        foreach (var anchor in rowAnchors)
        {
            var rowWords = tableWords
                .Where(word => Math.Abs(word.CenterY - anchor.CenterY) <= RowTolerance)
                .Where(word => ReferenceEquals(word, anchor) || IsNearestAnchor(word, anchor, rowAnchors))
                .OrderBy(word => word.Left)
                .ToList();

            var charged = ParseRightmostNumber(rowWords, columns.ChargedStart, columns.ChargedEnd);
            if (charged is null)
                continue;

            var service = JoinText(rowWords, 30, columns.UnitStart);
            if (string.IsNullOrWhiteSpace(service) || IsSummary(service))
                continue;

            var unit = JoinText(rowWords, columns.UnitStart, columns.VolumeStart);
            var volume = ParseRightmostNumber(rowWords, columns.VolumeStart, columns.TariffStart);
            var tariff = ParseRightmostNumber(rowWords, columns.TariffStart, columns.ChargedStart);

            yield return new ParsedBillRow(period, service, unit, volume, tariff, charged, sourceFile);
        }
    }

    private static bool IsNearestAnchor(
        PositionedWord word,
        PositionedWord currentAnchor,
        IReadOnlyList<PositionedWord> anchors)
    {
        var nearest = anchors
            .OrderBy(anchor => Math.Abs(anchor.CenterY - word.CenterY))
            .First();
        return ReferenceEquals(nearest, currentAnchor);
    }

    private static string JoinText(IEnumerable<PositionedWord> words, double from, double to)
    {
        var lines = words
            .Where(word => word.Left >= from && word.Left < to)
            .Where(word => !DatePattern.IsMatch(word.Text))
            .GroupBy(word => Math.Round(word.Bottom / 2))
            .OrderByDescending(line => line.Key)
            .Select(line => string.Join(" ", line.OrderBy(word => word.Left).Select(word => word.Text)));
        return string.Join(" ", lines).Trim();
    }

    private static decimal? ParseRightmostNumber(
        IEnumerable<PositionedWord> words,
        double from,
        double to)
    {
        var numericWords = words
            .Where(word => word.Left >= from && word.Left < to)
            .Where(word => NumberPattern.IsMatch(word.Text))
            .OrderBy(word => word.Left)
            .ToList();

        if (numericWords.Count == 0)
            return null;

        var selected = new List<PositionedWord> { numericWords[^1] };
        for (var index = numericWords.Count - 2; index >= 0; index--)
        {
            var current = numericWords[index];
            var right = selected[0];
            if (right.Left - current.Right > 5)
                break;
            selected.Insert(0, current);
        }

        var text = string.Concat(selected.Select(word => word.Text))
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(',', '.');
        return decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static bool TryGetPeriod(string fileName, out DateTime period)
    {
        period = default;
        if (fileName.Length < 6)
            return false;

        return DateTime.TryParseExact(
            fileName[..6],
            "yyyyMM",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out period);
    }

    private static bool IsSummary(string service)
    {
        return service.Contains("Итого", StringComparison.OrdinalIgnoreCase) ||
               service.Contains("К оплате", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record PositionedWord(string Text, double Left, double Right, double Bottom, double CenterY)
    {
        public static PositionedWord From(Word word)
        {
            return new PositionedWord(
                word.Text,
                word.BoundingBox.Left,
                word.BoundingBox.Right,
                word.BoundingBox.Bottom,
                (word.BoundingBox.Top + word.BoundingBox.Bottom) / 2);
        }
    }

    private readonly record struct TableColumns(
        double HeaderY,
        double UnitStart,
        double VolumeStart,
        double TariffStart,
        double ChargedStart,
        double ChargedEnd);
}
