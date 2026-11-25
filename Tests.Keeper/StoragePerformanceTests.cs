using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KeeperDomain;
using KeeperInfrastructure;

namespace Tests.Keeper;

[TestClass]
[DoNotParallelize]
public sealed class StoragePerformanceTests
{
    private const int WarmupRounds = 3;
    private const int MeasuredRounds = 20;

    private static readonly StorageFormat[] Formats = Enum.GetValues<StorageFormat>();

    private string? _reportPath;

    private static readonly JsonSerializerOptions CloneOptions = new() { IncludeFields = true };
    private static readonly PropertyInfo[] ListProperties = typeof(KeeperDomainModel).GetProperties()
        .Where(property => property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
        .OrderBy(property => property.Name)
        .ToArray();

    public TestContext TestContext { get; set; } = null!;

    [TestCleanup]
    public void AttachReport()
    {
        if (_reportPath != null && File.Exists(_reportPath))
        {
            TestContext.AddResultFile(_reportPath);
        }
    }

    [TestMethod]
    [TestCategory("Performance")]
    public async Task CompareTxtJsonAndCsvSaveLoadPerformance()
    {
        var setup = await PrepareAsync();
        Exception? failure = null;
        try
        {
            var measurements = new[] { "Load", "Save" }
                .SelectMany(operation => Formats.Select(format => new Measurement(operation, format)))
                .ToArray();

            for (var round = 0; round < WarmupRounds + MeasuredRounds; round++)
            {
                await RunRoundAsync(setup, measurements, round);
            }

            AssertFixturesUnchanged(setup);
            var report = BuildReport(setup, measurements);
            TestContext.WriteLine(report);
            var reportFolder = Path.Combine(FindProjectFolder(), "TestResults", "StoragePerformance",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(reportFolder);
            var reportPath = Path.GetFullPath(Path.Combine(reportFolder, "report.txt"));
            await File.WriteAllTextAsync(reportPath, report, new UTF8Encoding(false));
            _reportPath = reportPath;
            TestContext.WriteLine($"Report: {reportPath}");
        }
        catch (Exception error)
        {
            failure = error;
            throw;
        }
        finally
        {
            try
            {
                Directory.Delete(setup.TemporaryFolder, true);
            }
            catch (Exception cleanupError) when (failure != null)
            {
                TestContext.WriteLine($"Temporary directory cleanup failed: {setup.TemporaryFolder}: {cleanupError}");
            }
        }
    }

    private static async Task RunRoundAsync(BenchmarkSetup setup, Measurement[] measurements, int round)
    {
        var formatOrder = Enumerable.Range(0, Formats.Length)
            .Select(index => Formats[(round + index) % Formats.Length]).ToArray();
        if (round / Formats.Length % 2 != 0)
        {
            Array.Reverse(formatOrder);
        }

        var measured = round >= WarmupRounds;

        foreach (var format in formatOrder)
        {
            var folder = format switch
            {
                StorageFormat.TXT => setup.TxtFolder,
                StorageFormat.JSON => setup.JsonFolder,
                StorageFormat.CSV => setup.CsvFolder,
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
            var stopwatch = Stopwatch.StartNew();
            var model = await LoadAsync(folder, format);
            stopwatch.Stop();

            AssertCounts(setup.Counts, model, $"Round {round + 1}, {format} load");
            if (measured)
            {
                measurements.Single(sample => sample.Operation == "Load" && sample.Format == format)
                    .Milliseconds.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        var models = formatOrder.ToDictionary(format => format, _ => CloneModel(setup.Snapshot));
        foreach (var format in formatOrder)
        {
            var folder = Path.Combine(setup.TemporaryFolder, $"round-{round + 1}-{format}");
            Directory.CreateDirectory(folder);
            var source = models[format];

            var stopwatch = Stopwatch.StartNew();
            var error = await SaveAsync(source, folder, format);
            stopwatch.Stop();

            if (error != null)
            {
                throw new InvalidOperationException($"Round {round + 1}, {format} save failed.", error);
            }

            var restored = await LoadAsync(folder, format);
            AssertCounts(setup.Counts, restored, $"Round {round + 1}, {format} round trip");
            if (measured)
            {
                measurements.Single(sample => sample.Operation == "Save" && sample.Format == format)
                    .Milliseconds.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            Directory.Delete(folder, true);
        }
    }

    private static async Task<KeeperDomainModel> LoadAsync(string folder, StorageFormat format)
    {
        var model = format switch
        {
            StorageFormat.TXT => await KeeperDomainModelFromTxtLoader.LoadAllFromTextFiles(folder),
            StorageFormat.JSON => await KeeperDomainModelFromJsonLoader.LoadAllFromJsonFiles(folder),
            StorageFormat.CSV => await KeeperDomainModelFromCsvLoader.LoadAllFromCsvFiles(folder),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
        return model ?? throw new InvalidDataException($"Loader returned null for '{folder}'.");
    }

    private static Task<Exception?> SaveAsync(KeeperDomainModel model, string folder, StorageFormat format)
    {
        return format switch
        {
            StorageFormat.TXT => model.Save(folder),
            StorageFormat.JSON => model.SaveToJsonFiles(folder),
            StorageFormat.CSV => model.SaveToCsvFiles(folder),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static async Task<BenchmarkSetup> PrepareAsync()
    {
        var projectFolder = FindProjectFolder();
        var txtFolder = Path.Combine(projectFolder, "TxtFiles");
        var jsonFolder = Path.Combine(projectFolder, "JsonFiles");
        var csvFolder = Path.Combine(projectFolder, "CsvFiles");
        Assert.IsTrue(Directory.Exists(txtFolder), $"TXT fixture folder not found: {txtFolder}");
        Assert.IsTrue(Directory.Exists(jsonFolder), $"JSON fixture folder not found: {jsonFolder}");
        Assert.IsTrue(Directory.Exists(csvFolder), $"CSV fixture folder not found: {csvFolder}");
        var fixtureHashes = GetFixtureHashes(txtFolder, jsonFolder, csvFolder);

        var txtModel = await KeeperDomainModelFromTxtLoader.LoadAllFromTextFiles(txtFolder);
        Assert.IsNotNull(txtModel, "The TXT loader returned null.");
        var jsonModel = await KeeperDomainModelFromJsonLoader.LoadAllFromJsonFiles(jsonFolder);
        var csvModel = await KeeperDomainModelFromCsvLoader.LoadAllFromCsvFiles(csvFolder);
        var counts = GetCounts(jsonModel);
        AssertCounts(counts, txtModel, "TXT and JSON source fixtures");
        AssertCounts(counts, csvModel, "CSV and JSON source fixtures");

        var snapshot = JsonSerializer.SerializeToUtf8Bytes(jsonModel, CloneOptions);
        var temporaryFolder = Directory.CreateTempSubdirectory("KeeperStoragePerformance-").FullName;
        return new BenchmarkSetup(txtFolder, jsonFolder, csvFolder, temporaryFolder, snapshot, counts, fixtureHashes);
    }

    private static string FindProjectFolder()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Tests.Keeper.csproj")))
            {
                return directory.FullName;
            }

            var candidate = Path.Combine(directory.FullName, "Tests.Keeper");
            if (File.Exists(Path.Combine(candidate, "Tests.Keeper.csproj")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            $"Cannot locate Tests.Keeper and its fixture folders from '{AppContext.BaseDirectory}'.");
    }

    private static Dictionary<string, int> GetCounts(KeeperDomainModel model)
    {
        return ListProperties.ToDictionary(property => property.Name, property =>
            property.GetValue(model) is ICollection collection ? collection.Count :
                throw new InvalidDataException($"Collection '{property.Name}' is null or invalid."));
    }

    private static void AssertCounts(Dictionary<string, int> expected, KeeperDomainModel model, string context)
    {
        var actual = GetCounts(model);
        foreach (var (name, count) in expected)
        {
            Assert.AreEqual(count, actual[name], $"{context}: collection '{name}' has a different item count.");
        }
    }

    private static KeeperDomainModel CloneModel(byte[] snapshot)
    {
        return JsonSerializer.Deserialize<KeeperDomainModel>(snapshot, CloneOptions)
            ?? throw new InvalidDataException("Cannot restore the benchmark's source model.");
    }

    private static Dictionary<string, string> GetFixtureHashes(string txtFolder, string jsonFolder, string csvFolder)
    {
        return Directory.GetFiles(txtFolder, "*.txt").Concat(Directory.GetFiles(jsonFolder, "*.json"))
            .Concat(Directory.GetFiles(csvFolder, "*.csv"))
            .ToDictionary(path => path, path =>
            {
                using var stream = File.OpenRead(path);
                return Convert.ToHexString(SHA256.HashData(stream));
            });
    }

    private static void AssertFixturesUnchanged(BenchmarkSetup setup)
    {
        var hashes = GetFixtureHashes(setup.TxtFolder, setup.JsonFolder, setup.CsvFolder);
        Assert.HasCount(setup.FixtureHashes.Count, hashes, "The fixture file set changed during the experiment.");
        foreach (var (path, hash) in setup.FixtureHashes)
        {
            Assert.IsTrue(hashes.TryGetValue(path, out var actual), $"Fixture removed during the experiment: {path}");
            Assert.AreEqual(hash, actual, $"Fixture changed during the experiment: {path}");
        }
    }

    private static string BuildReport(BenchmarkSetup setup, Measurement[] measurements)
    {
        var report = new StringBuilder();
        var configuration = typeof(StoragePerformanceTests).Assembly
            .GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";
        report.AppendLine("Keeper TXT / JSON / CSV save and load performance");
        report.AppendLine(FormattableString.Invariant($"UTC: {DateTime.UtcNow:O}"));
        report.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}");
        report.AppendLine($"Build: {configuration}; debugger attached: {Debugger.IsAttached}");
        report.AppendLine(FormattableString.Invariant($"Warm-up rounds: {WarmupRounds}; measured rounds per operation/format: {MeasuredRounds}"));
        report.AppendLine(FormattableString.Invariant($"Stopwatch frequency: {Stopwatch.Frequency} ticks/second"));
        AppendFixtureInfo(report, "TXT", setup.TxtFolder, "*.txt");
        AppendFixtureInfo(report, "JSON", setup.JsonFolder, "*.json");
        AppendFixtureInfo(report, "CSV", setup.CsvFolder, "*.csv");
        report.AppendLine();
        report.AppendLine("Source list item counts (TXT, JSON and CSV counts match):");
        foreach (var (name, count) in setup.Counts)
        {
            report.AppendLine(FormattableString.Invariant($"  {name,-26} {count,10}"));
        }

        report.AppendLine(FormattableString.Invariant($"Total items: {setup.Counts.Values.Sum()}"));
        report.AppendLine();
        report.AppendLine("Operation Format       Min ms    Median ms      Mean ms       Max ms");
        report.AppendLine("--------- ------ ------------ ------------ ------------ ------------");
        foreach (var measurement in measurements)
        {
            Assert.HasCount(MeasuredRounds, measurement.Milliseconds,
                $"Incomplete measurements for {measurement.Operation} / {measurement.Format}.");
            report.AppendLine(FormattableString.Invariant(
                $"{measurement.Operation,-9} {measurement.Format,-6} {measurement.Milliseconds.Min(),12:F3} {Median(measurement.Milliseconds),12:F3} {measurement.Milliseconds.Average(),12:F3} {measurement.Milliseconds.Max(),12:F3}"));
        }

        report.AppendLine();
        foreach (var group in measurements.GroupBy(measurement => measurement.Operation))
        {
            var samples = group.ToArray();
            for (var first = 0; first < samples.Length; first++)
            {
                for (var second = first + 1; second < samples.Length; second++)
                {
                    AppendComparison(report, group.Key, samples[first], samples[second]);
                }
            }
        }
        report.AppendLine();
        report.AppendLine("Method and limitations:");
        report.AppendLine("- Sequential awaited wall-clock API calls; TXT/JSON/CSV order cycles through all six permutations.");
        report.AppendLine("- Load reads the original fixtures; save uses independent copies of one canonical JSON-loaded model.");
        report.AppendLine("- Each save starts with a new empty directory. Setup, model cloning, validation and cleanup are not timed.");
        report.AppendLine("- Generated exports are reloaded and all list counts checked outside timing; no field-by-field equality claim.");
        report.AppendLine("- SHA-256 checks confirmed all source fixture files remained unchanged.");
        report.AppendLine("- Windows filesystem caches are not cleared: warm-cache results, not cold-disk latency.");
        report.AppendLine("- Writes finish at API completion, without forcing a durable disk flush.");
        report.AppendLine("- TXT also sorts transactions and updates Account.ChildNumber; JSON writes indented data including public fields.");
        report.AppendLine("- JSON uses readable Unicode/plus signs. These compare current implementations, not pure serializer throughput.");
        report.AppendLine("- CSV uses CsvHelper with headers, dotted nested columns and invariant value conversion.");
        report.AppendLine("- No assertion requires a particular winner; differences below 1% of the slower median are reported as approximately tied.");
        report.AppendLine("- Prefer Release without a debugger and without concurrent workloads.");
        return report.ToString();
    }

    private static void AppendFixtureInfo(StringBuilder report, string format, string folder, string pattern)
    {
        var files = new DirectoryInfo(folder).GetFiles(pattern);
        report.AppendLine(FormattableString.Invariant(
            $"{format} input: {folder} ({files.Length} files, {files.Sum(file => file.Length)} bytes)"));
    }

    private static double Median(List<double> values)
    {
        var ordered = values.Order().ToArray();
        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) / 2 : ordered[middle];
    }

    private static void AppendComparison(StringBuilder report, string operation, Measurement first, Measurement second)
    {
        var firstMedian = Median(first.Milliseconds);
        var secondMedian = Median(second.Milliseconds);
        var slower = Math.Max(firstMedian, secondMedian);
        var faster = Math.Min(firstMedian, secondMedian);
        var label = $"{operation} ({first.Format} vs {second.Format})";
        if (slower - faster <= slower * 0.01)
        {
            report.AppendLine($"{label}: approximately tied (median difference <= 1%).");
        }
        else if (faster > 0)
        {
            var winner = firstMedian < secondMedian ? first.Format : second.Format;
            report.AppendLine(FormattableString.Invariant(
                $"{label}: {winner} faster; slower/faster median ratio = {slower / faster:F2}x."));
        }
        else
        {
            report.AppendLine($"{label}: a median is below timer resolution; ratio unavailable.");
        }
    }

    private enum StorageFormat
    {
        TXT,
        JSON,
        CSV
    }

    private sealed record BenchmarkSetup(string TxtFolder, string JsonFolder, string CsvFolder, string TemporaryFolder,
        byte[] Snapshot, Dictionary<string, int> Counts, Dictionary<string, string> FixtureHashes);

    private sealed class Measurement(string operation, StorageFormat format)
    {
        public string Operation { get; } = operation;
        public StorageFormat Format { get; } = format;
        public List<double> Milliseconds { get; } = new(MeasuredRounds);
    }
}
