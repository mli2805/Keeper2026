using Autofac;
using Caliburn.Micro;
using Microsoft.EntityFrameworkCore;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace KeeperWpf;

public sealed class AutofacWpf : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ShellViewModel>().As<IShell>();
        builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();

        // Register DbContext with SQLite
        builder.Register(c =>
        {
            var configuration = c.Resolve<IConfiguration>();
            var dbFolder = configuration["DataFolder"] ?? "";
            var dbPath = Path.Combine(dbFolder, "db/keeper.db");

            var optionsBuilder = new DbContextOptionsBuilder<KeeperDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            optionsBuilder.EnableSensitiveDataLogging();
            return new KeeperDbContext(optionsBuilder.Options);
        }).AsSelf().InstancePerLifetimeScope();

        builder.Register(c=>
        {
            var configuration = c.Resolve<IConfiguration>();
            var dbFolder = configuration["DataFolder"] ?? "";
            var logPath = Path.Combine(dbFolder, "logs/keeper.log");

            var logFile = new LogFile();
            logFile.AssignFile(logPath);
            LogHelper.LogFile = logFile;
            return logFile;
        }).AsSelf().InstancePerLifetimeScope();

        // Register DbContext Initializer
        builder.RegisterType<KeeperDbContextInitializer>().AsSelf();
        builder.RegisterType<LoadingProgressViewModel>().InstancePerLifetimeScope();
        builder.RegisterType<ToSqlite>().InstancePerLifetimeScope();
        builder.RegisterType<ToTxtSaver>().InstancePerLifetimeScope();

        builder.RegisterType<AccountRepository>().InstancePerLifetimeScope();
        builder.RegisterType<OfficialRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ExchangeRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<MetalRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<RefinancingRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<TransactionsRepository>().InstancePerLifetimeScope();
        builder.RegisterType<FuellingsRepository>().InstancePerLifetimeScope();
        builder.RegisterType<DepositOffersRepository>().InstancePerLifetimeScope();

        builder.RegisterType<TrustAccountsRepository>().InstancePerLifetimeScope();
        builder.RegisterType<TrustAssetsRepository>().InstancePerLifetimeScope();
        builder.RegisterType<TrustAssetRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<TrustTransactionsRepository>().InstancePerLifetimeScope();

        builder.RegisterType<SalaryChangesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<CardBalanceMemosRepository>().InstancePerLifetimeScope();
        builder.RegisterType<LargeExpenseThresholdsRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ButtonCollectionsRepository>().InstancePerLifetimeScope();

        builder.RegisterType<CarRepository>().InstancePerLifetimeScope();

        // глобальная модель данных приложения
        builder.RegisterType<KeeperDataModel>().InstancePerLifetimeScope();
        builder.RegisterType<KeeperDataModelInitializer>().InstancePerLifetimeScope();


        // Register ViewModels
        builder.RegisterType<ShellPartsBinder>().SingleInstance();
        builder.RegisterType<MainMenuViewModel>().SingleInstance();

        // AccountsTree view models
        builder.RegisterType<AccountTreeViewModel>().SingleInstance();
        builder.RegisterType<ComboTreesProvider>().SingleInstance();
        builder.RegisterType<AccNameSelector>().SingleInstance();
        builder.RegisterType<AskDragAccountActionViewModel>().SingleInstance();
        builder.RegisterType<OneAccountViewModel>().SingleInstance();
        builder.RegisterType<OneBankAccountViewModel>().SingleInstance();
        builder.RegisterType<DepositReportViewModel>().SingleInstance();
        builder.RegisterType<ExpensesOnAccountViewModel>().SingleInstance();
        builder.RegisterType<BalanceVerificationViewModel>().SingleInstance();
        builder.RegisterType<FolderSummaryViewModel>().SingleInstance();
        builder.RegisterType<DepositInterestViewModel>().SingleInstance();
        builder.RegisterType<CardFeeViewModel>().SingleInstance();
        builder.RegisterType<PaymentWaysViewModel>().SingleInstance();

        builder.RegisterType<BalanceOrTrafficViewModel>().SingleInstance();
        builder.RegisterType<TwoSelectorsViewModel>().SingleInstance();

        // Transactions view models
        builder.RegisterType<TransactionsViewModel>().SingleInstance();
        builder.RegisterType<TranModel>().SingleInstance();
        builder.RegisterType<FilterModel>().SingleInstance();
        builder.RegisterType<FilterViewModel>().SingleInstance();
        builder.RegisterType<TranEditExecutor>().SingleInstance();
        builder.RegisterType<TranMoveExecutor>().SingleInstance();
        builder.RegisterType<TranSelectExecutor>().SingleInstance();
        builder.RegisterType<OneTranViewModel>().SingleInstance();
        builder.RegisterType<ReceiptViewModel>().SingleInstance();
        builder.RegisterType<AskReceiptDeletionViewModel>().SingleInstance();
        builder.RegisterType<FuellingInputViewModel>().SingleInstance();
        builder.RegisterType<BalanceDuringTransactionHinter>().SingleInstance();
        builder.RegisterType<UniversalControlVm>();
        builder.RegisterType<NewExpenseControlVm>();
        
        // Month analysis view models
        builder.RegisterType<MonthAnalysisViewModel>();
        builder.RegisterType<MonthAnalyzer>();

        // 
        builder.RegisterType<BankOffersViewModel>();
        builder.RegisterType<OneBankOfferViewModel>();
        builder.RegisterType<RulesAndRatesViewModel>();


        // Charts view models
        builder.RegisterType<BalancesAndSaldosViewModel>();
        builder.RegisterType<DepoCurrResultViewModel>();
        builder.RegisterType<DepositCurrencySaldoCalculator>();
        builder.RegisterType<ExpenseByCategoriesViewModel>();
        builder.RegisterType<CategoriesDataExtractor>();

        // Cars view models
        builder.RegisterType<CarsViewModel>();
        builder.RegisterType<FuelViewModel>();
        builder.RegisterType<OwnershipCostViewModel>();

        // Memos view models
        builder.RegisterType<MemosViewModel>();
        builder.RegisterType<DateMemoSetterViewModel>();
        builder.RegisterType<CardPaymentsLimitsViewModel>();
        builder.RegisterType<CardBalanceMemoViewModel>();

        // Settings view models
        builder.RegisterType<SettingsViewModel>();
        builder.RegisterType<LargeExpenseThresholdViewModel>();
        builder.RegisterType<ButtonCollectionBuilderViewModel>();

        // Misc view models
        builder.RegisterType<MonthAnalysisViewModel>();
        builder.RegisterType<GskViewModel>();
        builder.RegisterType<SalaryViewModel>();
        builder.RegisterType<OpenDepositsViewModel>();
        builder.RegisterType<CardsAndAccountsViewModel>();

        // Rates view models
        builder.RegisterType<RatesViewModel>();
        builder.RegisterType<ExchangeRatesViewModel>();
        builder.RegisterType<OfficialRatesViewModel>();
        builder.RegisterType<GoldRatesViewModel>();
        builder.RegisterType<RefinancingRatesViewModel>();

        // Investments view models
        builder.RegisterType<InvestmentAssetsViewModel>();
        builder.RegisterType<OneAssetViewModel>();
        builder.RegisterType<AssetStatisticsViewModel>();
        builder.RegisterType<AssetRatesViewModel>();
        builder.RegisterType<AssetAnalysisViewModel>();
        builder.RegisterType<InvestmentAnalysisViewModel>();
        builder.RegisterType<InvestmentTransactionsViewModel>();
        builder.RegisterType<OneInvestTranViewModel>();
        builder.RegisterType<TrustAccountsViewModel>();
        builder.RegisterType<StatisticsLinesViewModel>();
        builder.RegisterType<TrustAccountStateViewModel>();
        builder.RegisterType<AssetsTableViewModel>();
        builder.RegisterType<TrustAccountAnalysisViewModel>();

    }
}
