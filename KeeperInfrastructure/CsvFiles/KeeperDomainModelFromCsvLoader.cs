using System.Collections;
using System.IO;
using CsvHelper;
using KeeperDomain;

namespace KeeperInfrastructure;

public static class KeeperDomainModelFromCsvLoader
{
    public static async Task<KeeperDomainModel> LoadAllFromCsvFiles(string backupFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFolder);
        var keeperDomainModel = new KeeperDomainModel();

        foreach (var (property, mapping) in KeeperDomainModelCsvConfiguration.Collections)
        {
            var filePath = Path.Combine(backupFolder, property.Name + ".csv");
            await using var stream = File.OpenRead(filePath);
            await using var csv = CsvReader.Create(stream, CsvReaderOptions.Default with
            {
                Separator = ',',
                TrimWhitespace = false,
                FieldCountMismatch = CsvFieldCountHandling.Throw,
                MalformedData = CsvMalformedDataHandling.Throw
            }, true);
            await csv.ReadHeaderAsync();
            var headers = csv.Headers.ToArray();
            var indices = mapping.Headers.Select(name => Array.IndexOf(headers, name)).ToArray();
            if (headers.Length != mapping.Headers.Length || indices.Any(index => index < 0))
            {
                throw new InvalidDataException(
                    $"Invalid CSV headers in '{filePath}'. Expected: {string.Join(", ", mapping.Headers)}.");
            }

            var collection = (IList)Activator.CreateInstance(property.PropertyType)!;
            while (await csv.ReadAsync())
            {
                try
                {
                    collection.Add(mapping.Read(csv, indices));
                }
                catch (InvalidDataException error)
                {
                    throw new InvalidDataException($"Invalid CSV record in '{filePath}', row {csv.RowNumber}.", error);
                }
            }

            property.SetValue(keeperDomainModel, collection);
        }

        return keeperDomainModel;
    }
}
