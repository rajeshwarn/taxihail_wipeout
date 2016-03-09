using System;

namespace apcurium.MK.Common
{
    public interface IConnectivityService
    {
        void HandleToastInNewView();
        void ToastDismissed();
        void ShowToast();
        bool IsConnected { get; }
    }
}

