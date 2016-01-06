using System;

namespace apcurium.MK.Common
{
    public interface IConnectivityService
    {
        IObservable<bool> GetAndObserveIsConnected();
        bool IsConnected { get; }
    }
}

