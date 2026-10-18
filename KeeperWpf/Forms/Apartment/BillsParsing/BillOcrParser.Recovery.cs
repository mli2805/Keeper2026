using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KeeperWpf;

public sealed partial class BillOcrParser
{
    private static void RecoverServiceRows(List<OcrWord> words, DateTime period, string fileName,
        List<ParsedBillRow> rows, List<BillParsingDiagnostic> diagnostics)
    {
        var dictionary = new BillOcrServiceDictionary();
        dictionary.Learn(rows);
        var width = words.Max(word => word.Left + word.Width);
        var heights = words.Where(word => Regex.IsMatch(word.Text, @"[а-яА-Я]") && word.Height > 5)
            .Select(word => word.Height).OrderBy(value => value).ToArray();
        if (heights.Length == 0) return;
        var height = heights[heights.Length / 2];
        var end = words.Where(word => word.Y > height * 10 &&
                (word.Text.StartsWith("ДОПОЛНИТЕЛЬНАЯ", StringComparison.OrdinalIgnoreCase) ||
                 word.Text.StartsWith("Расшифровка", StringComparison.OrdinalIgnoreCase) ||
                 word.Text.StartsWith("СПРАВОЧНАЯ", StringComparison.OrdinalIgnoreCase)))
            .Select(word => word.Y).DefaultIfEmpty(double.PositiveInfinity).Min();
        var candidates = words.Where(word => word.Left < width * 0.35 && word.Y < end &&
                word.Height >= height * 0.55 && Regex.IsMatch(word.Text, @"[а-яА-Я]") &&
                !Regex.IsMatch(word.Text, @"^\d{2}\.\d{2}\.\d{4}$"))
            .OrderBy(word => word.Y).ToList();
        var anchors = new List<ServiceAnchor>();
        foreach (var start in candidates)
        {
            if (anchors.Any(anchor => Math.Abs(anchor.Y - start.Y) < height * 0.6)) continue;
            var line = candidates.Where(word => word.Left >= start.Left &&
                    Math.Abs(word.Y - start.Y) <= height * 0.7)
                .OrderBy(word => word.Left).ToList();
            for (var count = Math.Min(7, line.Count); count > 0; count--)
            {
                var selected = line.Take(count).ToList();
                var text = string.Join(" ", selected.Select(word => word.Text));
                var match = dictionary.Match(text);
                if (match is not { } service) continue;
                var qualifier = words.Where(word => word.Left > selected[^1].Left &&
                        word.Left < width * 0.35 && Math.Abs(word.Y - start.Y) < height * 0.8)
                    .OrderBy(word => word.Left).Select(word => word.Text);
                var suffix = Regex.Match(string.Join(" ", qualifier), @"\(\s*\d+\s*[мmМM]\s*3\s*\)");
                var name = service.Name;
                if (name == "Подогрев воды" && suffix.Success) name += " " + suffix.Value;
                var baselineWords = selected.Where(word => word.Height <= height * 1.4).ToList();
                var y = baselineWords.Count > 0 ? baselineWords.Average(word => word.Y) : selected.Average(word => word.Y);
                var nearNumbers = words.Where(word => word.Left > width * 0.35 &&
                    Math.Abs(word.Y - y) < height * 0.65 &&
                    Regex.IsMatch(word.Text, @"^\d+[.,]\d+[|!\]}]*$")).ToList();
                var slope = 0.0;
                if (nearNumbers.Count >= 3 && nearNumbers.Max(word => word.Left) - nearNumbers.Min(word => word.Left) > width * 0.3)
                {
                    var meanX = nearNumbers.Average(word => word.Left);
                    var meanY = nearNumbers.Average(word => word.Y);
                    slope = nearNumbers.Sum(word => (word.Left - meanX) * (word.Y - meanY)) /
                        nearNumbers.Sum(word => (word.Left - meanX) * (word.Left - meanX));
                    if (Math.Abs(slope) > 0.015) slope = 0;
                    else y = meanY + slope * (start.Left - meanX);
                }
                anchors.Add(new(name, service.Unit, start.Left, y, slope));
                break;
            }
        }
        anchors = anchors.OrderBy(anchor => anchor.Y).ToList();
        if (anchors.Count == 0) return;
        var firstY = anchors[0].Y;
        var lastY = anchors[^1].Y;
        var units = words.Where(word => word.Y >= firstY - height && word.Y <= lastY + height &&
                word.Left > width * 0.2 && word.Left < width * 0.4 &&
                BillOcrServiceDictionary.NormalizeUnit(word.Text).Length > 0)
            .OrderBy(word => word.Left).ToList();
        var chargedHeader = words.Where(word => word.Y < firstY &&
                word.Text.StartsWith("Начислен", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(word => word.Left).FirstOrDefault();
        var totalHeader = words.FirstOrDefault(word => word.Y < firstY &&
            word.Text.StartsWith("Итог", StringComparison.OrdinalIgnoreCase));
        var modern = chargedHeader is not null && totalHeader is not null &&
            words.Any(word => word.Y < firstY && word.Text.StartsWith("Тариф**", StringComparison.OrdinalIgnoreCase));
        var unitLeft = units.Count > 0 ? units[units.Count / 2].Left : width * (modern ? 0.26 : 0.27);
        var numericWords = words.Where(word => word.Y >= firstY - height && word.Y <= lastY + height &&
            word.Left > unitLeft + height && word.Height < height * 3 &&
            (Regex.IsMatch(word.Text, @"\d") || word.Text is "-" or "−" or "–" or "=")) .ToList();
        var rightEdges = numericWords.Where(word => word.Left > width * 0.75 &&
                Regex.IsMatch(word.Text, @"\d[.,]\d{2}[|!\]}]*$"))
            .Select(word => word.Left + word.Width).OrderBy(value => value).ToArray();
        var totalRight = rightEdges.Length > 0 ? rightEdges[rightEdges.Length / 2] : width * 0.96;
        var span = totalRight - unitLeft;
        if (span <= 0) return;
        var boundaries = modern
            ? new[] { unitLeft + span * 0.07, unitLeft + span * 0.155, unitLeft + span * 0.27,
                unitLeft + span * 0.39, unitLeft + span * 0.515, unitLeft + span * 0.64,
                unitLeft + span * 0.765, unitLeft + span * 0.89 }
            : new[] { unitLeft + span * 0.08, unitLeft + span * 0.19, unitLeft + span * 0.325,
                unitLeft + span * 0.445, unitLeft + span * 0.57, unitLeft + span * 0.71,
                unitLeft + span * 0.845 };
        if (modern)
        {
            var scale = (totalHeader!.Left - chargedHeader!.Left) / 667;
            boundaries = new[] { chargedHeader.Left - 796 * scale, chargedHeader.Left - 630 * scale,
                chargedHeader.Left - 430 * scale, chargedHeader.Left - 420 * scale,
                chargedHeader.Left - 250 * scale, chargedHeader.Left - 20 * scale,
                chargedHeader.Left + 210 * scale, totalHeader.Left - 280 * scale,
                totalHeader.Left - 20 * scale };
        }
        var occurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var anchor in anchors)
        {
            double Distance(OcrWord word, ServiceAnchor service) =>
                Math.Abs(word.Y - (service.Y + service.Slope * (word.Left - service.Left)));
            var rowWords = numericWords.Where(word => Distance(word, anchor) <= height * 0.7 &&
                ReferenceEquals(anchors.OrderBy(service => Distance(word, service)).First(), anchor)).ToList();
            if (!rowWords.Any(word => Regex.IsMatch(word.Text, @"\d"))) continue;
            decimal? Read(double from, double to, string column) =>
                ReadNumber(rowWords, from, to, fileName, anchor.Name, column, diagnostics);
            var volume = Read(boundaries[0], boundaries[1], "Объем");
            var tariff = modern ? Read(boundaries[3], boundaries[4], "Тариф") : Read(boundaries[1], boundaries[2], "Тариф");
            var charged = modern ? Read(boundaries[5], boundaries[6], "Начислено") : Read(boundaries[2], boundaries[3], "Начислено");
            var recalculation = modern ? Read(boundaries[7], boundaries[8], "Перерасчет") : Read(boundaries[4], boundaries[5], "Перерасчет");
            var total = Read(modern ? boundaries[8] : boundaries[6], totalRight + height, "Итого");
            if (volume is null && tariff is null && charged is null && recalculation is null && total is null)
            {
                diagnostics.Add(new(fileName, $"{anchor.Name}: фрагменты цифр есть, но значения не удалось восстановить; строка сохранена."));
            }
            var baseName = Regex.Replace(anchor.Name, @"\s*\(\s*\d+\s*[мmМM]\s*3\s*\)", "");
            occurrences.TryGetValue(baseName, out var occurrence);
            occurrences[baseName] = occurrence + 1;
            var existing = rows.Select((row, index) => (row, index))
                .Where(pair => Regex.Replace(dictionary.Match(pair.row.ServiceName)?.Name ?? "",
                    @"\s*\(\s*\d+\s*[мmМM]\s*3\s*\)", "") == baseName).Skip(occurrence).FirstOrDefault();
            if (existing.row is not null)
            {
                rows[existing.index] = existing.row with
                {
                    ServiceName = dictionary.Match(existing.row.ServiceName)?.Name ?? anchor.Name, Unit = anchor.Unit,
                    Volume = existing.row.Volume ?? volume, Tariff = existing.row.Tariff ?? tariff,
                    Charged = existing.row.Charged ?? charged, Recalculation = existing.row.Recalculation ?? recalculation,
                    Total = existing.row.Total ?? total
                };
                continue;
            }
            rows.Add(new(period, anchor.Name, anchor.Unit, volume, tariff, charged, fileName, recalculation, total));
            diagnostics.Add(new(fileName, $"{anchor.Name}: строка восстановлена по расположению текста; проверьте числовые колонки."));
            if (charged is null || total is null || anchor.Unit != "руб." && (volume is null || tariff is null))
                diagnostics.Add(new(fileName, $"{anchor.Name}: строка распознана частично; отсутствующие значения оставлены пустыми."));
            if (charged is { } amount && total is { } result &&
                Math.Abs(amount + (recalculation ?? 0) - result) > 0.01m)
                diagnostics.Add(new(fileName, $"{anchor.Name}: начисление + перерасчет не совпадает с итогом; проверьте цифры, льготы и индексацию."));
        }
    }

    private sealed record ServiceAnchor(string Name, string Unit, double Left, double Y, double Slope);
}
