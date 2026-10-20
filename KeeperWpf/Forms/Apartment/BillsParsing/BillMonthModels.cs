using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Caliburn.Micro;

namespace KeeperWpf;

public sealed record BillFooter(DateTime Period, decimal? TotalAccrued = null,
    decimal? Carryover = null, decimal? Penalty = null,
    decimal? LandTax = null, decimal? TotalDue = null);

public sealed record BillMonthDocument(int Version, DateTime Period,
    IReadOnlyList<ParsedBillRow> Rows, BillFooter Footer, string SourceFile = "",
    UtilityMeters? UtilityMeters = null, string Address = "", string AccountNumber = "");

public abstract class BillEditableFields : PropertyChangedBase
{
    private readonly Dictionary<string, string> _values = new();
    protected string Get([CallerMemberName] string name = "") => _values.TryGetValue(name, out var value) ? value : string.Empty;
    protected void Set(string value, [CallerMemberName] string name = "")
    {
        if (Get(name) == value) return;
        _values[name] = value;
        NotifyOfPropertyChange(name);
    }
    public static string Format(decimal? value) => value?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
    public static bool TryNumber(string text, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        var normalized = text.Replace(" ", "").Replace("\u00a0", "").Replace("\u202f", "")
            .Replace(',', '.').Replace('−', '-');
        if (!decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var number)) return false;
        value = number;
        return true;
    }
    public static decimal? Number(string text)
    {
        if (!TryNumber(text, out var value)) throw new FormatException($"Некорректное число: {text}");
        return value;
    }
}

public sealed class BillEditableRow : BillEditableFields
{
    public new string Number
    {
        get => Get();
        set { Set(value); NotifyOfPropertyChange(nameof(SortNumber)); }
    }
    public int SortNumber => int.TryParse(Number, out var number) && number > 0 ? number : int.MaxValue;
    public string ServiceName { get => Get(); set => Set(value); }
    public string Unit { get => Get(); set => Set(value); }
    public string Volume { get => Get(); set => Set(value); }
    public string Tariff { get => Get(); set => Set(value); }
    public string Charged { get => Get(); set => Set(value); }
    public string Recalculation { get => Get(); set => Set(value); }
    public string Total { get => Get(); set => Set(value); }
    public bool IsValid => int.TryParse(Number, out var number) && number > 0 && !string.IsNullOrWhiteSpace(ServiceName) &&
        TryNumber(Volume, out _) && TryNumber(Tariff, out _) && TryNumber(Charged, out _) &&
        TryNumber(Recalculation, out _) && TryNumber(Total, out _);

    public static BillEditableRow From(ParsedBillRow row) => new()
    {
        Number = row.Number.ToString(CultureInfo.InvariantCulture),
        ServiceName = row.ServiceName, Unit = row.Unit, Volume = Format(row.Volume),
        Tariff = Format(row.Tariff), Charged = Format(row.Charged),
        Recalculation = Format(row.Recalculation), Total = Format(row.Total)
    };
    public ParsedBillRow ToRow(DateTime period) => new(period, ServiceName, Unit,
        BillEditableFields.Number(Volume), BillEditableFields.Number(Tariff), BillEditableFields.Number(Charged),
        string.Empty, BillEditableFields.Number(Recalculation), BillEditableFields.Number(Total), int.Parse(Number, CultureInfo.InvariantCulture));
}

public sealed class BillEditableFooter : BillEditableFields
{
    public string TotalAccrued { get => Get(); set => Set(value); }
    public string Carryover { get => Get(); set => Set(value); }
    public string Penalty { get => Get(); set => Set(value); }
    public string LandTax { get => Get(); set => Set(value); }
    public string TotalDue { get => Get(); set => Set(value); }
    public bool IsValid => TryNumber(TotalAccrued, out _) && TryNumber(Carryover, out _) &&
        TryNumber(Penalty, out _) && TryNumber(LandTax, out _) && TryNumber(TotalDue, out _);
    public static BillEditableFooter From(BillFooter footer) => new()
    {
        TotalAccrued = Format(footer.TotalAccrued), Carryover = Format(footer.Carryover),
        Penalty = Format(footer.Penalty), LandTax = Format(footer.LandTax), TotalDue = Format(footer.TotalDue)
    };
    public BillFooter ToFooter(DateTime period) => new(period, Number(TotalAccrued), Number(Carryover),
        Number(Penalty), Number(LandTax), Number(TotalDue));
}
