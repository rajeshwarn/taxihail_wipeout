using Microsoft.Practices.Unity;
using ServiceStack.Configuration;
using apcurium.MK.Common.Diagnostic;
using System.Linq;
namespace apcurium.MK.Common.IoC
{
    public class UnityContainerAdapter : IContainerAdapter
    {
        private readonly IUnityContainer _container;
        private readonly ILogger _logger;

        public UnityContainerAdapter(IUnityContainer container, ILogger logger)
        {
            _container = container;
            _logger = logger;
        }

        public T TryResolve<T>()
        {
            try
            {
                bool isRegistered   = _container.IsRegistered<T>();              
                if (isRegistered)
                {
                    return _container.Resolve<T>();
                }
                else
                {
                    //_logger.LogMessage("Warning: Failed resolution for " + typeof(T).Name);
                    return default(T);
                }
            }
            catch (ResolutionFailedException e)
            {
                _logger.LogMessage("Warning: Failed resolution for " + typeof(T).Name);
                return default(T);
            }
        }

        public T Resolve<T>()
        {
            try
            {
                return _container.Resolve<T>();
            }
            catch (ResolutionFailedException e)
            {
                _logger.LogMessage("Warning: Failed resolution for " + typeof(T).Name);
                return default(T);
            }
        }
    }
}