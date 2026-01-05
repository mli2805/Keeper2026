using Autofac;
using Caliburn.Micro;

namespace KeeperWpf
{
    public sealed class AutofacWpf : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();
        }
    }
}
