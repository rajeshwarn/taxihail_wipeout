#region

using apcurium.MK.Common.Diagnostic;
using Microsoft.Practices.Unity;
using ServiceStack.Configuration;

#endregion

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
                var isRegistered = _container.IsRegistered<T>();
                if (isRegistered)
                {
                    return _container.Resolve<T>();
                }
                //_logger.LogMessage("Warning: Failed resolution for " + typeof(T).Name);
                return default(T);
            }
            catch (ResolutionFailedException)
            {
                _logger.LogMessage("Warning: Failed resolution for " + typeof (T).Name);
                return default(T);
            }
        }

        public T Resolve<T>()
        {
            try
            {
                return _container.Resolve<T>();
            }
            catch (ResolutionFailedException)
            {
                _logger.LogMessage("Warning: Failed resolution for " + typeof (T).Name);
                return default(T);
            }
        }
    }
}