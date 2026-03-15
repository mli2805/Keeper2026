using KeeperDomain;
using System.IO;

namespace KeeperInfrastructure;

public static class TxtLoader
{
    private static string _backupFolder = null!;

    public static async Task<KeeperDomainModel?> LoadAllFromTextFiles(string backupFolder)
    {
        _backupFolder = backupFolder;
        var keeperDomainModel = new KeeperDomainModel
        {
            ExchangeRates = await ReadFileLines<ExchangeRates>(),
            OfficialRates = await ReadFileLines<OfficialRates>(),
            MetalRates = await ReadFileLines<MetalRate>(),
            RefinancingRates = await ReadFileLines<RefinancingRate>(),

            TrustAssets = await ReadFileLines<TrustAsset>(),
            TrustAssetRates = await ReadFileLines<TrustAssetRate>(),
            TrustAccounts = await ReadFileLines<TrustAccount>(),
            TrustTransactions = await ReadFileLines<TrustTransaction>(),

            AccountPlaneList = await ReadFileLines<Account>(),
            BankAccounts = await ReadFileLines<BankAccount>(),
            Deposits = await ReadFileLines<Deposit>(),
            PayCards = await ReadFileLines<PayCard>(),
            ButtonCollections = await ReadFileLines<ButtonCollection>(),

            DepositRateLines = await ReadFileLines<DepositRateLine>(),
            DepositConditions = await ReadFileLines<DepositConditions>(),
            DepositOffers = await ReadFileLines<DepositOffer>(),

            Transactions = await ReadFileLines<Transaction>(),
            Fuellings = await ReadFileLines<Fuelling>(),
            Cars = await ReadFileLines<Car>(),
            CarYearMileages = await ReadFileLines<CarYearMileage>(),

            SalaryChanges = await ReadFileLines<SalaryChange>(),
            LargeExpenseThresholds = await ReadFileLines<LargeExpenseThreshold>(),

            CardBalanceMemos = await ReadFileLines<CardBalanceMemo>("MemosCardBalance.txt"),

            // этих файлов не было в Keeper2018
            BankAccountMemos = await ReadFileLines<BankAccountMemo>(),
            CustomReminders = await ReadFileLines<CustomReminder>(),
        };
        return keeperDomainModel;
    }

    private static async Task<List<T>> ReadFileLines<T>(string filename = "") where T : KeeperDomain.IParsable<T>, new()
    {
        if (filename == "")
            filename = typeof(T).Name + "s.txt";

        if ((filename == "BankAccountMemos.txt" || filename == "CustomReminders.txt")
            && !File.Exists(Path.Combine(_backupFolder, filename)))
        {
            // Этих файлов не было в Keeper2018, поэтому если их нет, то просто возвращаем пустой список
            return new List<T>();
        }
        string[] fileContent = await File.ReadAllLinesAsync(Path.Combine(_backupFolder, filename));

        // Используем явное выделение памяти для списка, т.к. известен размер
        List<T> result = new List<T>(fileContent.Length);
        foreach (string line in fileContent)
        {
            result.Add(new T().FromString(line));
        }
        return result;
    }
}