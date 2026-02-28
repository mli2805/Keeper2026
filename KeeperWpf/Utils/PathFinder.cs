using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;

namespace KeeperWpf;

public class PathFinder(IMemoryCache memoryCache)
{
    private readonly string GoogleDriveKey = "GoogleDrivePath";

    public string GetDataFolder()
    {
        return Path.Combine(GetGoogleDrivePath(), "Keeper2026");
    }

    public string GetGoogleDrivePath()
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
