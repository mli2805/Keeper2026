using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KeeperWpf;

public sealed partial class BillOcrParser
{
    private static BillFooter ParseFooter(List<OcrWord> words, DateTime period, string fileName,
        List<BillParsingDiagnostic> diagnostics)
    {
        var end = words.Where(word => word.Text.StartsWith("ДОПОЛНИТЕЛЬНАЯ", StringComparison.OrdinalIgnoreCase) ||
                word.Text.StartsWith("Расшифровка", StringComparison.OrdinalIgnoreCase) ||
                word.Text.StartsWith("СПРАВОЧНАЯ", StringComparison.OrdinalIgnoreCase))
            .Select(word => word.Y).DefaultIfEmpty(double.PositiveInfinity).Min();
        var area = words.Where(word => word.Y < end).ToList();
        var width = area.Select(word => word.Left + word.Width).DefaultIfEmpty(0).Max();
        OcrWord? Find(string pattern) => area.Where(word => word.Left > width * 0.4 &&
            Regex.IsMatch(word.Text, pattern, RegexOptions.IgnoreCase)).OrderByDescending(word => word.Y).FirstOrDefault();
        var carryover = Find(@"^Переход");
        var penalty = Find(@"^[ПВ]еня$");
        var landTax = Find(@"^Возмещ");
        var totalDue = area.FirstOrDefault(word => word.Text.Equals("Всего", StringComparison.OrdinalIgnoreCase) &&
            area.Any(next => next.Left > word.Left && Math.Abs(next.Y - word.Y) < Math.Max(word.Height, next.Height) &&
                next.Text.Contains("оплат", StringComparison.OrdinalIgnoreCase)));
        var total = area.FirstOrDefault(word => Regex.IsMatch(word.Text, @"^Итого[:!|]?$", RegexOptions.IgnoreCase));

        decimal? Read(OcrWord? label, string name, bool summary = false)
        {
            if (label is null)
            {
                diagnostics.Add(new(fileName, $"Итоги: не распознана строка «{name}»."));
                return null;
            }
            var tolerance = Math.Max(15, label.Height * 0.9);
            var line = area.Where(word => word.Page == label.Page && word.Left >= label.Left &&
                    Math.Abs(word.Y - label.Y) <= tolerance && word.Height < label.Height * 1.8)
                .OrderBy(word => word.Left).ToList();
            var numeric = line.Where(word => Regex.IsMatch(word.Text, @"\d") &&
                !Regex.IsMatch(word.Text, @"\d{2}\.\d{2}\.\d{4}") &&
                word.Left > width * (summary ? 0.85 : 0.8)).OrderByDescending(word => word.Left).ToList();
            if (numeric.Count == 0)
            {
                diagnostics.Add(new(fileName, $"Итоги: отсутствует сумма «{name}»."));
                return null;
            }
            var last = numeric[0];
            var from = last.Left;
            var current = last;
            foreach (var previous in numeric.Skip(1))
            {
                if (current.Left - (previous.Left + previous.Width) > Math.Max(previous.Height, current.Height) * 1.5) break;
                from = previous.Left;
                current = previous;
            }
            var minus = line.Where(word => word.Left < from && word.Left + word.Width >= from - tolerance * 2 &&
                word.Text is "-" or "−" or "–" or "=").OrderByDescending(word => word.Left).FirstOrDefault();
            if (minus is not null) from = minus.Left;
            return ReadNumber(line, from, last.Left + last.Width + 1, fileName, "Итоги", name, diagnostics);
        }
        var accruedValue = Read(total, "Итого начислено", true);
        if (total is null && carryover is not null)
        {
            var nearby = area.Where(word => word.Page == carryover.Page && word.Y < carryover.Y - carryover.Height &&
                word.Y > carryover.Y - carryover.Height * 4 && word.Left > carryover.Left &&
                Regex.IsMatch(word.Text, @"\d+[.,]\d{2}")).OrderByDescending(word => word.Left).FirstOrDefault();
            if (nearby is not null)
            {
                accruedValue = Read(nearby with { Text = "Итого", Left = carryover.Left }, "Итого начислено", true);
                diagnostics.Add(new(fileName, "Итого начислено восстановлено по расположению над остатком; проверьте сумму."));
            }
        }
        var balanceValue = Read(carryover, "Переходящий остаток");
        var penaltyValue = Read(penalty, "Пеня");
        var taxValue = Read(landTax, "Возмещение земельного налога");
        var dueValue = Read(totalDue, "Всего к оплате");
        return new(period, accruedValue, balanceValue, penaltyValue, taxValue, dueValue);
    }
}
