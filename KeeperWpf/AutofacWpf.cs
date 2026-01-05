using Autofac;
using Caliburn.Micro;
using Microsoft.EntityFrameworkCore;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace KeeperWpf
{
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
                var dbFolder = configuration["Database:Folder"] ?? "";
                var dbPath = Path.Combine(dbFolder, "keeper.db");

                var optionsBuilder = new DbContextOptionsBuilder<KeeperDbContext>();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                return new KeeperDbContext(optionsBuilder.Options);
            }).AsSelf().InstancePerLifetimeScope();

            // Register DbContext Initializer
            builder.RegisterType<KeeperDbContextInitializer>().AsSelf();

            builder.RegisterType<CarRepository>().InstancePerLifetimeScope();
        }
    }
}
