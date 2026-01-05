using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Caliburn.Micro;

namespace KeeperWpf {
    public class AppBootstrapper : BootstrapperBase 
    {
        ILifetimeScope _container;

#pragma warning disable CS8618
        public AppBootstrapper()
        {
#pragma warning restore CS8618
            Initialize();
        }

        // protected override void Configure() 
        // {
        //   
        // }

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

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) 
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacWpf>();
            _container = builder.Build();

            DisplayRootViewForAsync<IShell>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(ShellView).Assembly; // this Assembly (.exe)
        }
    }
}