using Autofac;
using Caliburn.Micro;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KeeperWpf {
    public class AppBootstrapper : BootstrapperBase 
    {
        ILifetimeScope _container;
        IConfiguration _configuration;

#pragma warning disable CS8618
        public AppBootstrapper()
        {
#pragma warning restore CS8618
            Initialize();
        }

        protected override void Configure() 
        {
            // Build configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ?
                _container.Resolve(service) :
                _container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object>? GetAllInstances(Type service)
        {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }

        protected override async void OnStartup(object sender, System.Windows.StartupEventArgs e) 
        {
            var builder = new ContainerBuilder();
            
            // Register IConfiguration in Autofac
            builder.RegisterInstance(_configuration).As<IConfiguration>().SingleInstance();
            
            builder.RegisterModule<AutofacWpf>();
            _container = builder.Build();

            await DisplayRootViewForAsync<IShell>();

            // Initialize database
            var initializer = _container.Resolve<KeeperDbContextInitializer>();
            await initializer.InitializeAsync();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(ShellView).Assembly; // this Assembly (.exe)
        }
    }
}