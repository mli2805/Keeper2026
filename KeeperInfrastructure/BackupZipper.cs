using System.IO;
using System.IO.Compression;

namespace KeeperInfrastructure;

public static class BackupZipper
{
    public static async Task<Exception?> ZipFiles(string backupFolder, string extensionMask)
    {
        var archiveName = $"DB{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
        var zipFileToCreate = Path.Combine(backupFolder, archiveName);
        try
        {
            var zip = await ZipFile.OpenAsync(zipFileToCreate, ZipArchiveMode.Create);
            var filenames = Directory.GetFiles(backupFolder, extensionMask); // note: this does not recurse directories! 
            foreach (var filename in filenames)
            {
                await zip.CreateEntryFromFileAsync(filename, Path.GetFileName(filename), CompressionLevel.Optimal);
            }
            await zip.DisposeAsync();
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static Task<Exception?> DeleteFiles(string backupFolder, string extensionMask)
    {
        try
        {
            if (!Directory.Exists(backupFolder))
            {
                return Task.FromResult<Exception?>(new Exception("Backup directory does not exist"));
            }
            var filenames = Directory.GetFiles(backupFolder, extensionMask); // note: this does not recurse directories! 
            foreach (var filename in filenames)
                File.Delete(filename);
            return Task.FromResult<Exception?>(null);
        }
        catch (Exception e)
        {
            return Task.FromResult<Exception?>(e);
        }
    }
}
