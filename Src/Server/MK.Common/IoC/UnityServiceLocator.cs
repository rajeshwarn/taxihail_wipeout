#region
using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
#endregion

namespace apcurium.MK.Common.IoC
{
    public class UnityServiceLocator : ServiceLocatorImplBase
    {
        static UnityServiceLocator()
        {
            Initialize();
        }

        public UnityServiceLocator(IUnityContainer container)
        {
            Container = container;
        }

        public static IUnityContainer Instance { get; private set; }
        public IUnityContainer Container { get; private set; }

        public static IUnityContainer Initialize()
        {
            var container = new UnityContainer();

            var serviceLocator = Initialize(container);

            ServiceLocator.SetLocatorProvider(() => serviceLocator);

            Instance = container;

            return container;
        }

        public static IServiceLocator Initialize(IUnityContainer container)
        {
            container.AddNewExtension<UnityExtension>();

            var serviceLocator = new UnityServiceLocator(container);

            container.RegisterInstance<IServiceLocator>(serviceLocator);

            return serviceLocator;
        }

        /// <summary>
        ///     When implemented by inheriting classes, this method will do the actual work of resolving
        ///     the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>
        ///     The requested service instance.
        /// </returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType.IsInterface &&
                !UnityExtension.IsTypeRegistered(Container, serviceType))
            {
                return null;
            }
            return Container.Resolve(serviceType, key);
        }

        /// <summary>
        ///     When implemented by inheriting classes, this method will do the actual work of
        ///     resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>
        ///     Sequence of service instance objects.
        /// </returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return Container.ResolveAll(serviceType);
        }
    }
}