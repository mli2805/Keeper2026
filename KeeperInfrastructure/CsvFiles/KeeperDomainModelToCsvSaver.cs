using System.Collections;
using System.IO;
using CsvHelper;
using KeeperDomain;

namespace KeeperInfrastructure;

public static class KeeperDomainModelToCsvSaver
{
    public static async Task<Exception?> SaveToCsvFiles(this KeeperDomainModel keeperDomainModel, string backupFolder)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(keeperDomainModel);
            ArgumentException.ThrowIfNullOrWhiteSpace(backupFolder);
            Directory.CreateDirectory(backupFolder);

            foreach (var (property, mapping) in KeeperDomainModelCsvConfiguration.Collections)
            {
                var collection = property.GetValue(keeperDomainModel) as IEnumerable
                    ?? throw new InvalidDataException($"Collection '{property.Name}' is null.");
                var filePath = Path.Combine(backupFolder, property.Name + ".csv");
                await using var stream = File.Create(filePath);
                await using var csv = CsvWriter.Create(stream, CsvWriterOptions.Default with { Separator = ',' }, true);
                csv.WriteHeader(mapping.Headers);

                foreach (var record in collection)
                {
                    mapping.Write(csv, record);
                    await csv.EndRecordAsync();
                }
            }

            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
