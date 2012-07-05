using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.ObjectBuilder2;

namespace apcurium.MK.Web.IoC
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
            else
            {
                return IsTypeRegistered(container.Parent, type);
            }
        }

        private static bool IsTypeRegisteredInContainer(IUnityContainer container, Type type)
        {
            UnityExtension extension = container.Configure<UnityExtension>();

            if (extension == null)
            {
                return false;
            }
            else
            {
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

}
