using Autofac;
using HealthChecks.SomeModelService.AppServices.SomeModel;
using HealthChecks.SomeModelService.Repositories.SomeModel;

namespace HealthChecks.SomeModelService
{
    public class DependencyModule  : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SomeModelApplicationService>().As<ISomeModelApplicationService>();
            builder.RegisterType<SomeModelRepository>().As<ISomeModelRepository>();
            
        }
    }
}
