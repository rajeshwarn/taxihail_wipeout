#region

using System;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Common.IoC
{
    public class UnityExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
        }

        public static bool IsTypeRegistered(IUnityContainer container, Type type)
        {
            var registered = IsTypeRegisteredInContainer(container, type);

            if (registered ||
                container.Parent == null)
            {
                return registered;
            }
            return IsTypeRegistered(container.Parent, type);
        }

        private static bool IsTypeRegisteredInContainer(IUnityContainer container, Type type)
        {
            var extension = container.Configure<UnityExtension>();

            if (extension == null)
            {
                return false;
            }
            var key = new NamedTypeBuildKey(type);

            var mappingPolicy = extension
                .Context
                .Policies
                .Get<IBuildKeyMappingPolicy>(key);

            if (mappingPolicy != null)
            {
                return true;
            }

            var buildPlanPolicy = extension
                .Context
                .Policies
                .Get<IBuildPlanPolicy>(key);

            if (buildPlanPolicy != null &&
                buildPlanPolicy.GetType().Name == "FactoryDelegateBuildPlanPolicy")
            {
                return true;
            }

            return false;
        }
    }
}