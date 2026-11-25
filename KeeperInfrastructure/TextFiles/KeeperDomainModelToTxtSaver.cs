using System.IO;
using System.Text;
using KeeperDomain;

namespace KeeperInfrastructure;

public static  class KeeperDomainModelToTxtSaver
{
    public static async Task<Exception?> Save(this KeeperDomainModel keeperDomainModel, string backupFolder)
    {
        return await Task.Factory.StartNew(() => SaveTxtFiles(keeperDomainModel, backupFolder));
    }

    private static Exception? SaveTxtFiles(KeeperDomainModel keeperDomainModel, string backupFolder)
    {
        try
        {
            WriteFileLines(keeperDomainModel.OfficialRates, backupFolder);
            WriteFileLines(keeperDomainModel.ExchangeRates, backupFolder);
            WriteFileLines(keeperDomainModel.MetalRates, backupFolder);
            WriteFileLines(keeperDomainModel.RefinancingRates, backupFolder);

            WriteFileLines(keeperDomainModel.TrustAssets, backupFolder);
            WriteFileLines(keeperDomainModel.TrustAssetRates, backupFolder);
            WriteFileLines(keeperDomainModel.TrustAccounts, backupFolder);
            WriteFileLines(keeperDomainModel.TrustTransactions, backupFolder);

            File.WriteAllLines(
                Path.Combine(backupFolder, "Accounts.txt"),
                        DumpWithOffsets(keeperDomainModel.AccountPlaneList, backupFolder));
            WriteFileLines(keeperDomainModel.BankAccounts, backupFolder);
            WriteFileLines(keeperDomainModel.Deposits, backupFolder);
            WriteFileLines(keeperDomainModel.PayCards, backupFolder);

            var transactions = keeperDomainModel.Transactions
                .OrderBy(t => t.Timestamp)
                .Select(l => l.Dump());
            WriteTransactionsContent(Path.Combine(backupFolder, "Transactions.txt"), transactions);

            WriteFileLines(keeperDomainModel.Fuellings, backupFolder);

            WriteFileLines(keeperDomainModel.DepositOffers, backupFolder);
            WriteFileLines(keeperDomainModel.DepositRateLines, backupFolder);
            WriteFileLines(keeperDomainModel.DepositConditions, backupFolder);
            WriteFileLines(keeperDomainModel.MoneyBackRules, backupFolder);
            WriteFileLines(keeperDomainModel.MoneyBackCategories, backupFolder);
            WriteFileLines(keeperDomainModel.MoneyBackPartners, backupFolder);

            WriteFileLines(keeperDomainModel.Cars, backupFolder);
            WriteFileLines(keeperDomainModel.CarYearMileages, backupFolder);
            WriteFileLines(keeperDomainModel.UtilityMeters, backupFolder);
            WriteFileLines(keeperDomainModel.TodoTasks, backupFolder);
            WriteFileLines(keeperDomainModel.TodoSubtasks, backupFolder);

            WriteFileLines(keeperDomainModel.CardBalanceMemos, backupFolder, Path.Combine(backupFolder, "MemosCardBalance.txt"));
            WriteFileLines(keeperDomainModel.BankAccountMemos, backupFolder);

            WriteFileLines(keeperDomainModel.CustomReminders, backupFolder);
            WriteFileLines(keeperDomainModel.ButtonCollections, backupFolder);
            WriteFileLines(keeperDomainModel.SalaryChanges, backupFolder);
            WriteFileLines(keeperDomainModel.SalaryPayments, backupFolder);
            WriteFileLines(keeperDomainModel.LargeExpenseThresholds, backupFolder);

            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private static void WriteFileLines<T>(
        List<T> collection, string backupFolder, string filename = "") where T : IDumpable
    {
        if (filename == "")
        {
            filename = typeof(T).Name + "s.txt";
            filename = Path.Combine(backupFolder, filename);
        }
        var content = collection.Select(l => l.Dump());
        File.WriteAllLines(filename, content);
    }

    private static List<string> DumpWithOffsets(List<Account> accountPlainList, string backupFolder)
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
    private static void WriteTransactionsContent(string filename, IEnumerable<string> content)
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
}
