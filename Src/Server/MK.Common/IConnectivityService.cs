using System;

namespace apcurium.MK.Common
{
    public interface IConnectivityService
    {
        IObservable<bool> GetAndObserveIsConnected();
        void HandleToastInNewView();
        bool IsConnected { get; }
    }
}

