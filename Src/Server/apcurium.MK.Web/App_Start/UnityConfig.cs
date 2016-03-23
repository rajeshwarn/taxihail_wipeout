using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.App_Start
{
    public class UnityConfig
    {
        private static readonly Lazy<IUnityContainer> Container = new Lazy<IUnityContainer>(() =>
        {
            if (UnityServiceLocator.Instance == null)
            {
                UnityServiceLocator.Initialize();
            }
            return UnityServiceLocator.Instance;
        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return Container.Value;
        }
    }
}
