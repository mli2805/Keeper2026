using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

public sealed class BillParsingCsvStore
{
    private const string Header = "Период;Услуга;Единица измерения;Объем;Тариф;Начислено";

    public async Task SaveAsync(
        string filePath,
        IEnumerable<ParsedBillRow> rows,
        CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder(Header);
        builder.AppendLine();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            builder.Append(Escape(row.Period.ToString("yyyy-MM", CultureInfo.InvariantCulture))).Append(';')
                .Append(Escape(row.ServiceName)).Append(';')
                .Append(Escape(row.Unit)).Append(';')
                .Append(Format(row.Volume)).Append(';')
                .Append(Format(row.Tariff)).Append(';')
                .Append(Format(row.Charged))
                .AppendLine();
        }

        await File.WriteAllTextAsync(filePath, builder.ToString(), new UTF8Encoding(true), cancellationToken);
    }

    public async Task<IReadOnlyList<ParsedBillRow>> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
        if (lines.Length == 0 || lines[0].TrimStart('\uFEFF') != Header)
            throw new InvalidDataException("Файл не содержит ожидаемый заголовок таблицы жировок.");

        var rows = new List<ParsedBillRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(lines[index]))
                continue;

            var fields = ParseLine(lines[index]);
            if (fields.Count != 6 ||
                !DateTime.TryParseExact(fields[0], "yyyy-MM", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var period))
            {
                throw new InvalidDataException($"Некорректная строка CSV: {index + 1}.");
            }

            rows.Add(new ParsedBillRow(
                period,
                fields[1],
                fields[2],
                ParseNullableDecimal(fields[3], index),
                ParseNullableDecimal(fields[4], index),
                ParseNullableDecimal(fields[5], index),
                string.Empty));
        }

        return rows;
    }

    private static string Format(decimal? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static decimal? ParseNullableDecimal(string value, int lineIndex)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out var result))
            return result;

        throw new InvalidDataException($"Некорректное число в строке CSV: {lineIndex + 1}.");
    }

    private static string Escape(string value)
    {
        if (!value.Contains(';') && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static IReadOnlyList<string> ParseLine(string line)
    {
        var fields = new List<string>();
        var value = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ';' && !inQuotes)
            {
                fields.Add(value.ToString());
                value.Clear();
            }
            else
            {
                value.Append(character);
            }
        }

        fields.Add(value.ToString());
        return fields;
    }
}
