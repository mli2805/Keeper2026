using Microsoft.Extensions.Caching.Memory;
using System.IO;

namespace KeeperInfrastructure;

public class PathFinder(IMemoryCache memoryCache)
{
    private readonly string GoogleDriveKey = "GoogleDrivePath";

    public string GetDataFolder()
    {
        return Path.Combine(GetGoogleDrivePath(), "Keeper2026");
    }

    public string GetBackupFolder()
    {
        return Path.Combine(GetDataFolder(), "backup");
    }

    public string GetJsonBackupFolder()
    {
        return Path.Combine(GetDataFolder(), "jsonBackup");
    }

    public string GetCsvBackupFolder()
    {
        return Path.Combine(GetDataFolder(), "csvBackup");
    }

    private string GetGoogleDrivePath()
    {
        if (memoryCache.TryGetValue(GoogleDriveKey, out string? path))
        {
            return path!;
        }
        else
        {
            string googleDrivePath = FindGoogleDrivePath();
            memoryCache.Set(GoogleDriveKey, googleDrivePath);
            return googleDrivePath;
        }
    }

    private static string FindGoogleDrivePath()
    {
        if (Directory.Exists(@"d:\Google Drive"))
        {
            return @"d:\Google Drive";
        }
        if (Directory.Exists(@"c:\Google Drive"))
        {
            return @"c:\Google Drive";
        }

        throw new Exception("РџР°РїРєР° Google Drive РЅРµ РЅР°Р№РґРµРЅР°!");
    }
}
