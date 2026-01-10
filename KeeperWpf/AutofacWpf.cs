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

        // Register DbContext Initializer
        builder.RegisterType<KeeperDbContextInitializer>().AsSelf();
        builder.RegisterType<LoadingProgressViewModel>().InstancePerLifetimeScope();
        builder.RegisterType<ToSqlite>().InstancePerLifetimeScope();

        builder.RegisterType<AccountRepository>().InstancePerLifetimeScope();
        builder.RegisterType<OfficialRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ExchangeRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<MetalRatesRepository>().InstancePerLifetimeScope();
        builder.RegisterType<RefinancingRatesRepository>().InstancePerLifetimeScope();

        builder.RegisterType<CarRepository>().InstancePerLifetimeScope();

        // глобальная модель данных приложения
        builder.RegisterType<KeeperDataModel>().InstancePerLifetimeScope();
        builder.RegisterType<KeeperDataModelInitializer>().InstancePerLifetimeScope();


        // Register ViewModels
        builder.RegisterType<ShellPartsBinder>().SingleInstance();

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

        builder.RegisterType<RatesViewModel>();
        builder.RegisterType<ExchangeRatesViewModel>();
        builder.RegisterType<OfficialRatesViewModel>();
        builder.RegisterType<GoldRatesViewModel>();
        builder.RegisterType<RefinancingRatesViewModel>();
    }
}
