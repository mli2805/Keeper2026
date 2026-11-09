using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeeperWpf;

public static class MccCodesParser
{
    public static List<MccCodeItem> Parse(string csv)
    {
        var rows = ParseCsv(csv);

        var result = new List<MccCodeItem>(rows.Count);
        for (var i = 1; i < rows.Count; i++) // skip header
        {
            var row = rows[i];
            if (row.Count == 0) continue;

            result.Add(new MccCodeItem
            {
                Code = row[0],
                Name = row.Count > 1 ? row[1] : string.Empty,
                Description = row.Count > 2 ? row[2] : string.Empty,
            });
        }
        return result;
    }

    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                row.Add(field.ToString());
                field.Clear();
                rows.Add(row);
                row = new List<string>();
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }
}
