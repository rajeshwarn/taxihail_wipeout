using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IMessageService
    {
        void ShowMessage(string title, string message);

        void ShowToast(string message, ToastDuration duration);
    }

    public enum ToastDuration
    {
        Short,
        Long,
    }
}
