using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KeeperWpf;

internal sealed class BillOcrServiceDictionary
{
    private readonly Dictionary<string, string> _units = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Тех.обслуживание"] = "кв.м.",
        ["Отч. на кап.ремонт"] = "кв.м.",
        ["Капитальный ремонт"] = "кв.м.",
        ["Отопление"] = "Гкал",
        ["Вода-канализация"] = "куб.м",
        ["Подогрев воды"] = "Гкал",
        ["Вывоз ТБО"] = "куб.м",
        ["Вывоз ТКО"] = "куб.м",
        ["Обезвреживание ТБО"] = "куб.м",
        ["Обезвреж.и переработ.ТКО"] = "куб.м",
        ["Обращение с ТКО"] = "куб.м",
        ["Пользование лифтом"] = "чел",
        ["Технич.обслуж.лифта"] = "чел",
        ["Радио"] = "шт",
        ["ТВ-программы"] = "шт",
        ["Техобслуживание ЗПУ"] = "руб.",
        ["Информ.услуги(абон.плата)"] = "руб.",
        ["Сан.содерж.вспом.пом.ж/д"] = "чел",
        ["Э/э осв.всп.пом.,раб.обор"] = "кВт*ч",
        ["Электроэн.движения лифта"] = "кВт*ч"
    };

    public void Learn(IEnumerable<ParsedBillRow> rows)
    {
        foreach (var group in rows.GroupBy(row => row.ServiceName.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Select(row => row.Period).Distinct().Count() >= 3))
        {
            if (Match(group.Key) is not null) continue;
            var unit = group.Select(row => NormalizeUnit(row.Unit)).Where(value => value.Length > 0)
                .GroupBy(value => value).OrderByDescending(values => values.Count()).FirstOrDefault();
            if (unit is not null) _units.TryAdd(group.Key, unit.Key);
        }
    }

    public (string Name, string Unit)? Match(string text)
    {
        text = text.Trim(' ', '|', '!', '[', ']', '{', '}', '\\');
        var qualifier = Regex.Match(text, @"\(\s*\d+\s*[мm]\s*3\s*\)", RegexOptions.IgnoreCase);
        var baseText = qualifier.Success ? text.Remove(qualifier.Index, qualifier.Length).Trim() : text;
        var key = Key(baseText);
        var matches = _units.Select(pair => (pair.Key, pair.Value, Distance: Distance(key, Key(pair.Key))))
            .Where(pair => pair.Distance <= (key.Length < 7 ? 1 : 2))
            .OrderBy(pair => pair.Distance).ToList();
        if (matches.Count == 0 || matches.Count > 1 && matches[0].Distance == matches[1].Distance) return null;
        var best = matches[0];
        return (best.Key + (qualifier.Success ? " " + qualifier.Value.Replace('m', 'м') : ""), best.Value);
    }

    public static string NormalizeUnit(string text)
    {
        var key = Key(text).Replace('k', 'к').Replace('b', 'в').Replace('m', 'м');
        return key switch
        {
            "квм" => "кв.м.",
            "кубм" or "куб" => "куб.м",
            "гкал" => "Гкал",
            "чел" => "чел",
            "шт" or "lut" => "шт",
            "руб" => "руб.",
            "квтч" => "кВт*ч",
            _ => string.Empty
        };
    }

    private static string Key(string text) => Regex.Replace(text.ToLowerInvariant().Replace('ё', 'е'), @"[^а-яa-z0-9]", "");

    private static int Distance(string left, string right)
    {
        if (Math.Abs(left.Length - right.Length) > 2) return 3;
        var previous = Enumerable.Range(0, right.Length + 1).ToArray();
        for (var i = 1; i <= left.Length; i++)
        {
            var current = new int[right.Length + 1];
            current[0] = i;
            for (var j = 1; j <= right.Length; j++)
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1));
            previous = current;
        }
        return previous[^1];
    }
}
