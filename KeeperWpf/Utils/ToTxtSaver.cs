using KeeperDomain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeperWpf;

public class ToTxtSaver
{
    private string _backupFolder;
    private readonly KeeperDataModel _keeperDataModel;

    public ToTxtSaver(IConfiguration configuration, KeeperDataModel keeperDataModel)
    {
        var dataFolderPath = configuration["DataFolder"];
        _backupFolder = dataFolderPath != null ? Path.Combine(dataFolderPath, "backup") : @"../backup";
        _keeperDataModel = keeperDataModel;
    }

    public async Task<Exception?> Save()
    {
        var keeperModel = _keeperDataModel.From();
        return await Task.Factory.StartNew(() => SaveTxtFiles(keeperModel));
    }

    private Exception? SaveTxtFiles(KeeperDomainModel keeperModel)
    {
        try
        {
            WriteFileLines(keeperModel.OfficialRates);
            WriteFileLines(keeperModel.ExchangeRates);
            WriteFileLines(keeperModel.MetalRates);
            WriteFileLines(keeperModel.RefinancingRates);

            WriteFileLines(keeperModel.TrustAssets);
            WriteFileLines(keeperModel.TrustAssetRates);
            WriteFileLines(keeperModel.TrustAccounts);
            WriteFileLines(keeperModel.TrustTransactions);


            File.WriteAllLines(Path.Combine(_backupFolder, "Accounts.txt"), DumpWithOffsets(keeperModel.AccountPlaneList));
            WriteFileLines(keeperModel.BankAccounts);
            WriteFileLines(keeperModel.Deposits);
            WriteFileLines(keeperModel.PayCards);

            var transactions = keeperModel.Transactions
                .OrderBy(t => t.Timestamp)
                .Select(l => l.Dump());
            WriteTransactionsContent(Path.Combine(_backupFolder, "Transactions.txt"), transactions);

            WriteFileLines(keeperModel.Fuellings);

            WriteFileLines(keeperModel.DepositOffers);
            WriteFileLines(keeperModel.DepositRateLines);
            WriteFileLines(keeperModel.DepositConditions);

            WriteFileLines(keeperModel.Cars);
            WriteFileLines(keeperModel.CarYearMileages);

            WriteFileLines(keeperModel.CardBalanceMemos, Path.Combine(_backupFolder, "MemosCardBalance.txt"));
            WriteFileLines(keeperModel.ButtonCollections);
            WriteFileLines(keeperModel.SalaryChanges);
            WriteFileLines(keeperModel.LargeExpenseThresholds);

            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private void WriteFileLines<T>(List<T> collection, string filename = "") where T : IDumpable
    {
        if (filename == "")
        {
            filename = typeof(T).Name + "s.txt";
            filename = Path.Combine(_backupFolder, filename);
        }
        var content = collection.Select(l => l.Dump());
        File.WriteAllLines(filename, content);
    }

    private List<string> DumpWithOffsets(List<Account> accountPlainList)
    {
        var result = new List<string>();
        var previousParents = new Stack<(int, int)>();
        previousParents.Push((0, 0));
        var previousAccountId = 0;
        var level = 0;
        foreach (var account in accountPlainList)
        {
            while (account.ParentId != previousParents.Peek().Item1)
            {
                if (account.ParentId == previousAccountId)
                {
                    level++;
                    previousParents.Push((previousAccountId, 0));
                }
                else
                {
                    level--;
                    previousParents.Pop();
                }
            }

            var currentParent = previousParents.Pop();
            currentParent.Item2++;
            account.ChildNumber = currentParent.Item2;
            previousParents.Push(currentParent);


            var dump = account.Dump();
            result.Add(dump.Insert(dump.IndexOf(';') + 1, new string(' ', level * 2)));
            previousAccountId = account.Id;
        }
        return result;
    }

    // supposedly it should be faster than File.WriteAllLines because of increased buffer
    private void WriteTransactionsContent(string filename, IEnumerable<string> content)
    {
        if (File.Exists(filename)) File.Delete(filename);

        const int bufferSize = 65536;  // 64 Kilobytes
        using (var sw = new StreamWriter(filename, true, Encoding.UTF8, bufferSize))
        {
            foreach (var str in content)
            {
                sw.WriteLine(str);
            }
        }
    }

    public async Task<Exception?> ZipTxtFiles()
    {
        var archiveName = $"DB{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
        var zipFileToCreate = Path.Combine(_backupFolder, archiveName);
        try
        {
            var zip = await ZipFile.OpenAsync(zipFileToCreate, ZipArchiveMode.Create);
            var filenames = Directory.GetFiles(_backupFolder, "*.txt"); // note: this does not recurse directories! 
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

    public async Task<Exception?> DeleteTxtFiles()
    {
        try
        {
            if (!Directory.Exists(_backupFolder))
            {
                return new Exception("Backup directory does not exist");
            }
            var filenames = Directory.GetFiles(_backupFolder, "*.txt"); // note: this does not recurse directories! 
            foreach (var filename in filenames)
                File.Delete(filename);
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
