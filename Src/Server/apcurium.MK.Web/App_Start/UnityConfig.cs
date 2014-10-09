using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.App_Start
{
    public class UnityConfig
    {
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            return UnityServiceLocator.Instance;
            var container = new UnityContainer();
            return container;
        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
    }
}
