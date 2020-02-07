using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using F4ST.Common.Mappers;

namespace F4ST.Common.Containers
{
    public class IoC : WindsorServiceProviderFactory
    {
        public static WindsorContainer Container;

        public static void Install(params IIoCInstaller[] installers)
        {
            if (Container == null)
                Container = new WindsorContainer();

            Container.Register(Component.For<IInterceptor>().ImplementedBy<LoggingMethodInterceptor>());
            Container.Register(Component.For<IMapper>().ImplementedBy<Mapper>());

            var mapper = Resolve<IMapper>();

            foreach (var installer in installers)
            {
                installer.Install(Container, mapper);

                Container.Register(Classes.FromAssembly(installer.GetType().Assembly)
                    .BasedOn<ISingleton>()
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    .LifestyleSingleton()
                    /*.Configure(c => c.Interceptors<LoggingMethodInterceptor, CacheMethodInterceptor>().CrossWired())*/);

                Container.Register(Classes.FromAssembly(installer.GetType().Assembly)
                    .BasedOn<IPerScope>()
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    //.LifestyleScoped()
                    .LifestyleCustom<MsScopedLifestyleManager>()
                    /*.Configure(c => c.Interceptors<LoggingMethodInterceptor, CacheMethodInterceptor>().CrossWired())*/);

                Container.Register(Classes.FromAssembly(installer.GetType().Assembly)
                    .BasedOn<ITransient>()
                    .WithService.Self()
                    .WithService.DefaultInterfaces()
                    .LifestyleTransient()
                    /*.Configure(c => c.Interceptors<LoggingMethodInterceptor, CacheMethodInterceptor>().CrossWired())*/);

            }

        }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }


    }

}