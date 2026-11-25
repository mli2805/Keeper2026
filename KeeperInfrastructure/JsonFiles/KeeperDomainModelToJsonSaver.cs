using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using KeeperDomain;

namespace KeeperInfrastructure;

public static class KeeperDomainModelToJsonSaver
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<Exception?> SaveToJsonFiles(this KeeperDomainModel keeperDomainModel, string backupFolder)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(keeperDomainModel);
            Directory.CreateDirectory(backupFolder);

            foreach (var property in typeof(KeeperDomainModel).GetProperties())
            {
                if (!property.PropertyType.IsGenericType ||
                    property.PropertyType.GetGenericTypeDefinition() != typeof(List<>))
                {
                    continue;
                }

                var filePath = Path.Combine(backupFolder, property.Name + ".json");
                await using var stream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(
                    stream, property.GetValue(keeperDomainModel), property.PropertyType, SerializerOptions);
            }

            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
