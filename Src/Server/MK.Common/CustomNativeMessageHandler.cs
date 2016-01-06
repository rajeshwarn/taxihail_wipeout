using System;
using ModernHttpClient;
using System.Net;

namespace apcurium.MK.Common
{
    public class CustomNativeMessageHandler : NativeMessageHandler
    {
        private readonly IConnectivityService _connectivityService;

        public CustomNativeMessageHandler(IConnectivityService connectivityService, bool throwOnCaptiveNetwork, bool customSSLVerification, NativeCookieHandler cookieHandler = null)
            : base(throwOnCaptiveNetwork, customSSLVerification, cookieHandler)
        {
            _connectivityService = connectivityService;
        }

        public CustomNativeMessageHandler(IConnectivityService connectivityService)
        {
            _connectivityService = connectivityService;
        }

        protected override System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if(!_connectivityService.IsConnected)
            {
                throw new WebException("NoNetwork", WebExceptionStatus.ConnectFailure);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

