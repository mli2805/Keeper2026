using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KeeperWpf;

public sealed class BillMonthJsonStore
{
    public const string DirectoryPath = @"C:\temp\bills\jpg\res";
    public const string PdfDirectoryPath = @"C:\temp\bills\res";
    private readonly string _sourceExtension;

    public BillMonthJsonStore(string storageDirectoryPath = DirectoryPath, string sourceExtension = ".jpeg")
    {
        StorageDirectoryPath = storageDirectoryPath;
        _sourceExtension = sourceExtension;
    }

    public string StorageDirectoryPath { get; }
    public string GetPath(DateTime period) => Path.Combine(StorageDirectoryPath,
        period.ToString("yyyyMM", CultureInfo.InvariantCulture) + ".json");

    public string GetSourceFile(DateTime period) => period.ToString("yyyyMM", CultureInfo.InvariantCulture) + _sourceExtension;

    public static UtilityMeters GetUtilityMeters(BillMonthDocument document)
    {
        if (document.UtilityMeters is { } existing) return existing;
        return TryCalculateWaterMeters(document, out var meters) ? meters : CreateUtilityMeters(document.Period);
    }

    public static bool TryCalculateWaterMeters(BillMonthDocument document, out UtilityMeters meters)
    {
        meters = document.UtilityMeters ?? CreateUtilityMeters(document.Period);
        var heating = document.Rows.Select(row => Regex.Match(row.ServiceName,
            @"(?:Подогрев\s+воды|Гор\.\s*водоснаб\.\s*\(\s*подогрев\s*\))\s*\(\s*(?<volume>\d+(?:[.,]\d+)?)\s*[мm]\s*[3³]\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Where(match => match.Success).ToArray();
        var water = document.Rows.Where(row => Regex.IsMatch(row.ServiceName,
            @"^\s*Вода\s*[-–—]\s*канализация\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            .ToArray();
        if (heating.Length != 1 ||
            !decimal.TryParse(heating[0].Groups["volume"].Value.Replace(',', '.'),
                NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var hot))
            return false;
        meters.HotWater.Volume = (double)hot;
        var total = water.Sum(row => row.Volume ?? 0);
        if (water.Length > 0 && total >= hot)
            meters.ColdWater.Volume = (double)(total - hot);
        return true;
    }

    private static UtilityMeters CreateUtilityMeters(DateTime period) => new()
    {
        ReadingsDate = DateOnly.FromDateTime(period.AddMonths(-1)).AddDays(20)
    };

    public async Task SaveAsync(BillMonthDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.SourceFile))
            document = document with { SourceFile = GetSourceFile(document.Period) };
        Validate(document);
        document = document with { UtilityMeters = GetUtilityMeters(document) };
        var path = GetPath(document.Period);
        Directory.CreateDirectory(StorageDirectoryPath);
        var settings = new JsonSerializerSettings { ContractResolver = new BillJsonContractResolver() };
        var json = JsonConvert.SerializeObject(document, Formatting.Indented, settings);
        var temporary = path + ".tmp";
        await File.WriteAllTextAsync(temporary, json, new UTF8Encoding(false));
        File.Move(temporary, path, true);
    }

    public async Task<BillMonthDocument> LoadAsync(DateTime period)
    {
        var json = await File.ReadAllTextAsync(GetPath(period));
        return Deserialize(json, period);
    }

    public BillMonthDocument Load(DateTime period) => Deserialize(File.ReadAllText(GetPath(period)), period);

    private BillMonthDocument Deserialize(string json, DateTime period)
    {
        var document = JsonConvert.DeserializeObject<BillMonthDocument>(json)
            ?? throw new InvalidDataException("Пустой файл месячного счета.");
        if (document.Rows is not null)
            document = document with
            {
                SourceFile = string.IsNullOrWhiteSpace(document.SourceFile) ? GetSourceFile(document.Period) : document.SourceFile,
                Rows = document.Rows.Select(row => row is null ? row! : row with
                {
                    Period = row.Period == default ? document.Period : row.Period,
                    SourceFile = string.Empty
                }).ToArray()
            };
        Validate(document);
        if (document.Period != period) throw new InvalidDataException("Период файла не совпадает с выбранным месяцем.");
        return document;
    }

    private static void Validate(BillMonthDocument document)
    {
        if (document.Version != 1 || document.Period.Day != 1 || document.Period.TimeOfDay != TimeSpan.Zero ||
            document.Footer is null || document.Rows is null || document.Footer.Period != document.Period ||
            document.Rows.Any(row => row is null || row.Period != document.Period || string.IsNullOrWhiteSpace(row.ServiceName)))
            throw new InvalidDataException("Некорректный формат месячного счета.");
    }

    private sealed class BillJsonContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(
            System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.DeclaringType == typeof(ParsedBillRow) &&
                member.Name is nameof(ParsedBillRow.MonthBackground) or nameof(ParsedBillRow.Period) or nameof(ParsedBillRow.SourceFile))
                property.Ignored = true;
            return property;
        }
    }
}
