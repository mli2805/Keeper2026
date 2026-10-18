using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

public sealed partial class BillOcrParser
{
    public Task<BillParsingResult> ParseDirectoryAsync(string directoryPath,
        IProgress<(int Processed, int Total, string FileName)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var rows = new List<ParsedBillRow>();
            var diagnostics = new List<BillParsingDiagnostic>();
            var footers = new List<BillFooter>();
            var files = Directory.EnumerateFiles(directoryPath)
                .Where(path => Path.GetExtension(path).Equals(".tsv", StringComparison.OrdinalIgnoreCase))
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).ToArray();
            for (var index = 0; index < files.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = Path.GetFileName(files[index]);
                try
                {
                    var stem = Path.GetFileNameWithoutExtension(files[index]);
                    if (!DateTime.TryParseExact(stem, "yyyyMM", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var period))
                        throw new InvalidDataException("Имя TSV должно иметь формат yyyyMM.tsv.");
                    ParseFile(files[index], period, rows, diagnostics, footers, cancellationToken);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception exception) { diagnostics.Add(new(name, exception.Message)); }
                finally { progress?.Report((index + 1, files.Length, name)); }
            }
            var dictionary = new BillOcrServiceDictionary();
            dictionary.Learn(rows);
            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var match = dictionary.Match(row.ServiceName);
                if (match is not { } service) continue;
                if (row.ServiceName != service.Name || row.Unit != service.Unit)
                    diagnostics.Add(new(row.SourceFile,
                        $"Словарь услуг: «{row.ServiceName}» → «{service.Name}», единица «{service.Unit}»."));
                rows[index] = row with { ServiceName = service.Name, Unit = service.Unit };
            }
            var numberedRows = rows.GroupBy(row => row.Period)
                .SelectMany(group => group.Select((row, index) => row with { Number = index + 1 })).ToArray();
            return new BillParsingResult(numberedRows, diagnostics, footers);
        }, cancellationToken);
    }

    private static void ParseFile(string path, DateTime period, List<ParsedBillRow> rows,
        List<BillParsingDiagnostic> diagnostics, List<BillFooter> footers, CancellationToken cancellationToken)
    {
        var words = new List<OcrWord>();
        var lineNumber = 0;
        foreach (var line in File.ReadLines(path))
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineNumber++;
            var fields = line.Split('\t', 12);
            if (lineNumber == 1)
            {
                if (!line.TrimStart('\uFEFF').StartsWith("level\tpage_num\t", StringComparison.Ordinal))
                    throw new InvalidDataException("Не найден заголовок Tesseract TSV.");
                continue;
            }
            if (fields.Length != 12)
                throw new InvalidDataException($"Некорректная строка TSV: {lineNumber}.");
            if (fields[0] != "5" || string.IsNullOrWhiteSpace(fields[11])) continue;
            if (!int.TryParse(fields[1], out var page) ||
                !double.TryParse(fields[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out var left) ||
                !double.TryParse(fields[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out var top) ||
                !double.TryParse(fields[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) ||
                !double.TryParse(fields[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
                throw new InvalidDataException($"Некорректные координаты TSV: {lineNumber}.");
            words.Add(new(fields[11].Trim(), page, left, top, width, height));
        }
        var fileName = Path.GetFileName(path);
        var initialCount = rows.Count;
        foreach (var page in words.GroupBy(word => word.Page))
            ParsePage(page.ToList(), period, fileName, rows, diagnostics);
        footers.Add(ParseFooter(words, period, fileName, diagnostics));
        if (rows.Count == initialCount)
            diagnostics.Add(new(fileName, "Строки начислений не найдены. Проверьте OCR и формат таблицы."));
    }

    private static void ParsePage(List<OcrWord> words, DateTime period, string fileName,
        List<ParsedBillRow> rows, List<BillParsingDiagnostic> diagnostics)
    {
        var pageRows = new List<ParsedBillRow>();
        var pageDiagnostics = new List<BillParsingDiagnostic>();
        ParseHeaderPage(words, period, fileName, pageRows, pageDiagnostics);
        RecoverServiceRows(words, period, fileName, pageRows, diagnostics);
        rows.AddRange(pageRows);
        if (pageRows.Count > 0) diagnostics.AddRange(pageDiagnostics.Where(diagnostic =>
            !diagnostic.Message.Contains("таблица не разобрана", StringComparison.Ordinal) &&
            !diagnostic.Message.Contains("повторный OCR", StringComparison.Ordinal)));
    }

    private static void ParseHeaderPage(List<OcrWord> words, DateTime period, string fileName,
        List<ParsedBillRow> rows, List<BillParsingDiagnostic> diagnostics)
    {
        var totalHeader = words.FirstOrDefault(word => word.Text.StartsWith("Итог", StringComparison.OrdinalIgnoreCase));
        if (totalHeader is null)
        {
            diagnostics.Add(new(fileName, "Не распознан заголовок итоговой суммы; таблица не разобрана."));
            return;
        }
        var headerWords = words.Where(word => Math.Abs(word.Y - totalHeader.Y) < totalHeader.Height * 2).ToList();
        var chargedHeader = headerWords.Where(word => word.Text.StartsWith("Начислен", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(word => word.Left).FirstOrDefault();
        var tariffHeader = headerWords.Where(word => word.Text.StartsWith("Тариф", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(word => word.Left).FirstOrDefault();
        if (chargedHeader is null || tariffHeader is null || totalHeader.Left <= chargedHeader.Left)
        {
            diagnostics.Add(new(fileName, "Не распознаны заголовки начисления/тарифа; требуется повторный OCR."));
            return;
        }
        var modern = headerWords.Count(word => word.Text.StartsWith("Начислен", StringComparison.OrdinalIgnoreCase)) > 1;
        var scale = modern ? (totalHeader.Left - chargedHeader.Left) / 667 : tariffHeader.Width / 103;
        var unitHeader = headerWords.FirstOrDefault(word => word.Text.StartsWith("Ед", StringComparison.OrdinalIgnoreCase));
        var volumeHeader = headerWords.FirstOrDefault(word => word.Text.StartsWith("Кол", StringComparison.OrdinalIgnoreCase) ||
            word.Text.StartsWith("Объ", StringComparison.OrdinalIgnoreCase));
        if (!modern && (unitHeader is null || volumeHeader is null))
        {
            diagnostics.Add(new(fileName, "Не распознаны колонки единицы измерения/объема старого формата."));
            return;
        }
        var unitStart = modern ? chargedHeader.Left - 910 * scale : unitHeader!.Left - 20 * scale;
        var volumeStart = modern ? chargedHeader.Left - 796 * scale : volumeHeader!.Left - 20 * scale;
        var tariffStart = tariffHeader.Left - 25 * scale;
        var chargedStart = chargedHeader.Left - 20 * scale;
        var totalStart = totalHeader.Left - 20 * scale;
        var recalculationStart = modern ? totalHeader.Left - 280 * scale : totalStart - 210 * scale;
        var chargedEnd = modern ? chargedHeader.Left + 210 * scale : recalculationStart - 210 * scale;
        var tolerance = Math.Max(12 * scale, totalHeader.Height);
        var footer = words.Where(word => word.Y > totalHeader.Y + tolerance &&
                (word.Text.StartsWith("ИТОГО", StringComparison.OrdinalIgnoreCase) ||
                 word.Text.StartsWith("Тарифы", StringComparison.OrdinalIgnoreCase) ||
                 word.Text.StartsWith("Переходящий", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(word => word.Y).FirstOrDefault();
        var endY = footer?.Y ?? words.Where(word => word.Text.StartsWith("ДОПОЛНИТЕЛЬНАЯ", StringComparison.OrdinalIgnoreCase))
            .Select(word => word.Y).DefaultIfEmpty(words.Max(word => word.Y)).Min();
        var body = words.Where(word => word.Y > totalHeader.Y + tolerance * 2 && word.Y < endY - tolerance).ToList();
        var anchors = body.Where(word => word.Left >= chargedStart && word.Left < chargedEnd &&
                Regex.IsMatch(word.Text, @"\d"))
            .OrderBy(word => word.Y).ToList();
        var rowYs = new List<double>();
        foreach (var anchor in anchors)
            if (rowYs.Count == 0 || anchor.Y - rowYs[^1] > tolerance)
                rowYs.Add(anchor.Y);
        var pageRows = new List<ParsedBillRow>();
        foreach (var y in rowYs)
        {
            var rowWords = body.Where(word => Math.Abs(word.Y - y) <= tolerance &&
                rowYs.OrderBy(rowY => Math.Abs(rowY - word.Y)).First() == y).ToList();
            var serviceWords = rowWords.Where(word => word.Left >= unitStart - 510 * scale &&
                word.Left < unitStart && word.Height >= 22 * scale &&
                Math.Abs(word.Y - y) <= 20 * scale &&
                (Regex.IsMatch(word.Text, @"[а-яА-Яa-zA-Z]") || Regex.IsMatch(word.Text, @"^\(\d+$")) &&
                !Regex.IsMatch(word.Text, @"^\d{2}\.\d{2}\.\d{4}$")).OrderBy(word => word.Left).ToList();
            var service = string.Join(" ", serviceWords.Select(word => word.Text.Trim('|', '!', '}', '{')));
            if (string.IsNullOrWhiteSpace(service))
            {
                diagnostics.Add(new(fileName, $"Не распознано название услуги возле Y={y:0}."));
                continue;
            }
            decimal? Read(double from, double to, string column) => ReadNumber(rowWords, from, to,
                fileName, service, column, diagnostics);
            var unit = string.Join(" ", rowWords.Where(word => word.Left >= unitStart && word.Left < volumeStart &&
                    Regex.IsMatch(word.Text, @"[а-яА-Яa-zA-Z]"))
                .OrderBy(word => word.Left).Select(word => word.Text.Trim('|', '!', '}', '{')));
            var volume = Read(volumeStart, modern ? chargedHeader.Left - 630 * scale : tariffStart, "Объем");
            var tariff = Read(tariffStart, modern ? chargedHeader.Left - 250 * scale : chargedStart, "Тариф");
            var charged = Read(chargedStart, chargedEnd, "Начислено");
            var recalculation = Read(recalculationStart, totalStart, "Перерасчет");
            var total = Read(totalStart, double.PositiveInfinity, "Итого");
            if (charged is null || total is null)
                diagnostics.Add(new(fileName, $"{service}: отсутствует начисление или итог; проверьте строку."));
            if (charged is { } amount && total is { } result && Math.Abs(amount + (recalculation ?? 0) - result) > 0.01m)
                diagnostics.Add(new(fileName, $"{service}: начисление + перерасчет не совпадает с итогом (возможна льгота или ошибка OCR)."));
            if (string.IsNullOrEmpty(unit))
                diagnostics.Add(new(fileName, $"{service}: не распознана единица измерения."));
            else if (!unit.StartsWith("руб", StringComparison.OrdinalIgnoreCase) && (volume is null || tariff is null))
                diagnostics.Add(new(fileName, $"{service}: не распознан объем или тариф; проверьте строку."));
            pageRows.Add(new(period, service, unit, volume, tariff, charged, fileName, recalculation, total));
        }
        rows.AddRange(pageRows);
        var summary = words.Where(word => word.Y >= endY - tolerance &&
                word.Y < endY + 4 * tolerance &&
                word.Text.StartsWith("ИТОГО", StringComparison.OrdinalIgnoreCase))
            .OrderBy(word => word.Y).FirstOrDefault();
        if (summary is not null && pageRows.Count > 0)
        {
            var footerWords = words.Where(word => Math.Abs(word.Y - summary.Y) <= tolerance).ToList();
            var expected = ReadNumber(footerWords, chargedStart, chargedEnd, fileName, "ИТОГО", "Начислено", diagnostics);
            if (expected is { } sum && (pageRows.Any(row => row.Charged is null) ||
                    Math.Abs(pageRows.Sum(row => row.Charged ?? 0) - sum) > 0.01m))
                diagnostics.Add(new(fileName, "Сумма начислений строк не совпадает с ИТОГО; возможны пропущенные строки."));
            var expectedTotal = ReadNumber(footerWords, totalStart, double.PositiveInfinity,
                fileName, "ИТОГО", "Итого", diagnostics);
            if (expectedTotal is { } totalSum && (pageRows.Any(row => row.Total is null) ||
                    Math.Abs(pageRows.Sum(row => row.Total ?? 0) - totalSum) > 0.01m))
                diagnostics.Add(new(fileName, "Сумма итогов строк не совпадает с ИТОГО."));
        }
        else if (pageRows.Count > 0)
            diagnostics.Add(new(fileName, "Не распознана строка ИТОГО для контроля полноты начислений."));
    }

    private static decimal? ReadNumber(List<OcrWord> words, double from, double to, string fileName,
        string service, string column, List<BillParsingDiagnostic> diagnostics)
    {
        var cell = words.Where(word => word.Left >= from && word.Left < to)
            .OrderBy(word => word.Left).ToList();
        var numeric = cell.Where(word => Regex.IsMatch(word.Text, @"\d")).ToList();
        if (numeric.Count == 0) return null;
        if (numeric.Zip(numeric.Skip(1)).Any(pair =>
                pair.Second.Left - (pair.First.Left + pair.First.Width) > Math.Max(pair.First.Height, pair.Second.Height) * 2))
        {
            diagnostics.Add(new(fileName, $"{service}, {column}: несколько раздельных чисел в одной ячейке."));
            return null;
        }
        var raw = string.Concat(numeric.Select(word => word.Text));
        var normalized = Regex.Replace(raw, @"[\s|\[\]{}!]", "").Replace(',', '.');
        var sign = cell.Any(word => word.Text is "-" or "−" or "–") ? "-" : "";
        if (cell.Any(word => word.Text == "=") ||
            !Regex.IsMatch(normalized, @"^-?\d+(?:\.\d+)?$") ||
            !decimal.TryParse(sign + normalized.TrimStart('-'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
        {
            diagnostics.Add(new(fileName, $"{service}, {column}: неоднозначное число «{string.Join(" ", cell.Select(word => word.Text))}»."));
            return null;
        }
        if (normalized.StartsWith('-')) value = -Math.Abs(value);
        return value;
    }

    private sealed record OcrWord(string Text, int Page, double Left, double Top, double Width, double Height)
    {
        public double Y => Top + Height / 2;
    }
}
