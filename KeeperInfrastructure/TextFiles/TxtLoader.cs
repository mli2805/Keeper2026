using KeeperDomain;

namespace KeeperInfrastructure;

public static class TxtLoader
{
    private static string _backupFolder;

    public static async Task<KeeperModel?> LoadAllFromTextFiles(string backupFolder)
    {
        _backupFolder = backupFolder;
        var keeperModel = new KeeperModel
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

            CardBalanceMemos = await ReadFileLines<CardBalanceMemo>("MemosCardBalance.txt"),
            SalaryChanges = await ReadFileLines<SalaryChange>(),
            LargeExpenseThresholds = await ReadFileLines<LargeExpenseThreshold>(),
        };
        return keeperModel;
    }

    private static async Task<List<T>> ReadFileLines<T>(string filename = "") where T : KeeperDomain.IParsable<T>, new()
    {
        if (filename == "")
            filename = typeof(T).Name + "s.txt";

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