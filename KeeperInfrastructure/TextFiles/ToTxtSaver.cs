using System.IO;
using System.IO.Compression;
using System.Text;
using KeeperDomain;

namespace KeeperInfrastructure;

public class ToTxtSaver
{
    private string _backupFolder;

    public ToTxtSaver(PathFinder pathFinder)
    {
        var dataFolderPath = pathFinder.GetDataFolder();
        _backupFolder = Path.Combine(dataFolderPath, "backup");
    }

    public async Task<Exception?> Save(KeeperDomainModel keeperDomainModel)
    {
        return await Task.Factory.StartNew(() => SaveTxtFiles(keeperDomainModel));
    }

    private Exception? SaveTxtFiles(KeeperDomainModel keeperDomainModel)
    {
        try
        {
            WriteFileLines(keeperDomainModel.OfficialRates);
            WriteFileLines(keeperDomainModel.ExchangeRates);
            WriteFileLines(keeperDomainModel.MetalRates);
            WriteFileLines(keeperDomainModel.RefinancingRates);

            WriteFileLines(keeperDomainModel.TrustAssets);
            WriteFileLines(keeperDomainModel.TrustAssetRates);
            WriteFileLines(keeperDomainModel.TrustAccounts);
            WriteFileLines(keeperDomainModel.TrustTransactions);


            File.WriteAllLines(Path.Combine(_backupFolder, "Accounts.txt"), DumpWithOffsets(keeperDomainModel.AccountPlaneList));
            WriteFileLines(keeperDomainModel.BankAccounts);
            WriteFileLines(keeperDomainModel.Deposits);
            WriteFileLines(keeperDomainModel.PayCards);

            var transactions = keeperDomainModel.Transactions
                .OrderBy(t => t.Timestamp)
                .Select(l => l.Dump());
            WriteTransactionsContent(Path.Combine(_backupFolder, "Transactions.txt"), transactions);

            WriteFileLines(keeperDomainModel.Fuellings);

            WriteFileLines(keeperDomainModel.DepositOffers);
            WriteFileLines(keeperDomainModel.DepositRateLines);
            WriteFileLines(keeperDomainModel.DepositConditions);
            WriteFileLines(keeperDomainModel.MoneyBackRules);
            WriteFileLines(keeperDomainModel.MoneyBackCategories);
            WriteFileLines(keeperDomainModel.MoneyBackPartners);

            WriteFileLines(keeperDomainModel.Cars);
            WriteFileLines(keeperDomainModel.CarYearMileages);
            WriteFileLines(keeperDomainModel.UtilityMeters);
            WriteFileLines(keeperDomainModel.TodoTasks);
            WriteFileLines(keeperDomainModel.TodoSubtasks);

            WriteFileLines(keeperDomainModel.CardBalanceMemos, Path.Combine(_backupFolder, "MemosCardBalance.txt"));
            WriteFileLines(keeperDomainModel.BankAccountMemos);

            WriteFileLines(keeperDomainModel.CustomReminders);
            WriteFileLines(keeperDomainModel.ButtonCollections);
            WriteFileLines(keeperDomainModel.SalaryChanges);
            WriteFileLines(keeperDomainModel.SalaryPayments);
            WriteFileLines(keeperDomainModel.LargeExpenseThresholds);

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

    public Task<Exception?> DeleteTxtFiles()
    {
        try
        {
            if (!Directory.Exists(_backupFolder))
            {
                return Task.FromResult<Exception?>(new Exception("Backup directory does not exist"));
            }
            var filenames = Directory.GetFiles(_backupFolder, "*.txt"); // note: this does not recurse directories! 
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
