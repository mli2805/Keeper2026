using System.IO;
using System.Text.Json;
using KeeperDomain;

namespace KeeperInfrastructure;

public static class KeeperDomainModelFromJsonLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        IncludeFields = true
    };

    public static async Task<KeeperDomainModel> LoadAllFromJsonFiles(string backupFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFolder);
        var keeperDomainModel = new KeeperDomainModel();

        foreach (var property in typeof(KeeperDomainModel).GetProperties())
        {
            if (!property.PropertyType.IsGenericType ||
                property.PropertyType.GetGenericTypeDefinition() != typeof(List<>))
            {
                continue;
            }

            var filePath = Path.Combine(backupFolder, property.Name + ".json");
            await using var stream = File.OpenRead(filePath);
            var collection = await JsonSerializer.DeserializeAsync(stream, property.PropertyType, SerializerOptions)
                ?? throw new JsonException($"Expected a JSON array in '{filePath}', but found null.");
            property.SetValue(keeperDomainModel, collection);
        }

        return keeperDomainModel;
    }
}
