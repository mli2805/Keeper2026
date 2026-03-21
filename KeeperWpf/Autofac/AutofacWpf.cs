using Autofac;
using Caliburn.Micro;
using KeeperInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Linq;

namespace KeeperWpf;

public sealed class AutofacWpf : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ShellViewModel>().As<IShell>();
        builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();

        builder.Register(_ => new MemoryCache(new MemoryCacheOptions()))
            .As<IMemoryCache>()
            .SingleInstance();

        builder.RegisterType<PathFinder>().SingleInstance();

        // Register Factory for DbContext with SQLite
        // так же как в Asp.net, где DbContext создается на время жизни 1 запроса,
        // в WPF приложении создаем DbContext при каждом запросе (при каждом вызове функции любого репозитория)
        // Смысл тот же - чтобы не накапливались изменения в трекинге EF, не росло потребление памяти
        builder.Register(c =>
        {
            //var configuration = c.Resolve<IConfiguration>();
            //var dbFolder = configuration["DataFolder"] ?? "";
            //var dbPath = Path.Combine(dbFolder, "db/keeper.db");

            var pathFinder = c.Resolve<PathFinder>();
            var dataFolder = pathFinder.GetDataFolder();
            var dbPath = Path.Combine(dataFolder, "db/keeper.db");

            var options = new DbContextOptionsBuilder<KeeperDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            return new KeeperDbContextFactory(options);
        })
        .As<IDbContextFactory<KeeperDbContext>>()
        .SingleInstance();

        builder.Register(c =>
        {
            //var configuration = c.Resolve<IConfiguration>();
            //var dbFolder = configuration["DataFolder"] ?? "";
            //var logPath = Path.Combine(dbFolder, "logs/keeper.log");

            var pathFinder = c.Resolve<PathFinder>();
            var dataFolder = pathFinder.GetDataFolder();
            var logPath = Path.Combine(dataFolder, "logs/keeper.log");

            var logFile = new LogFile();
            logFile.AssignFile(logPath);
            LogHelper.LogFile = logFile;
            return logFile;
        }).AsSelf().InstancePerLifetimeScope();

        // Register DbContext Initializer
        builder.RegisterType<KeeperDbContextInitializer>().AsSelf();

        RegisterRepositories(builder);

        builder.RegisterType<ToSqlite>().InstancePerLifetimeScope();
        builder.RegisterType<ToTxtSaver>().InstancePerLifetimeScope();

        // глобальная модель данных приложения
        builder.RegisterType<KeeperDataModel>().InstancePerLifetimeScope();
        builder.RegisterType<KeeperDataModelInitializer>().InstancePerLifetimeScope();

        RegisterViewModels(builder);
    }

    private static void RegisterRepositories(ContainerBuilder builder)
    {
        var assembly = typeof(AccountRepository).Assembly;
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                        && t.IsDefined(typeof(ExportRepositoryAttribute), true));

        foreach (var type in repositoryTypes)
        {
            builder.RegisterType(type).InstancePerLifetimeScope();
        }
    }

    private static void RegisterViewModels(ContainerBuilder builder)
    {
        var assembly = typeof(ShellViewModel).Assembly;
        var viewModelTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Type = t,
                Attr = (ExportViewModelAttribute?)t
                    .GetCustomAttributes(typeof(ExportViewModelAttribute), true)
                    .FirstOrDefault()
            })
            .Where(x => x.Attr != null);

        foreach (var x in viewModelTypes)
        {
            var reg = builder.RegisterType(x.Type);
            switch (x.Attr!.Lifetime)
            {
                case ViewModelLifetime.SingleInstance:
                    reg.SingleInstance();
                    break;
            }
        }
    }
}
