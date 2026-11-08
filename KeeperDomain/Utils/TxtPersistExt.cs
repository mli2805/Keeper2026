using System.Globalization;

namespace KeeperDomain;

public static class TxtPersistExt
{
    public static string DumpNullableDecimal(this decimal? value)
    {
        return value?.ToString(new CultureInfo("en-US")) ?? string.Empty;
    }

    public static decimal? ParseNullableDecimal(this string s)
    {
        var trimmed = s.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : decimal.Parse(trimmed, CultureInfo.InvariantCulture);
    }
}
