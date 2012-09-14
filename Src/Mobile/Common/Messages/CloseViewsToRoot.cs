using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class CloseViewsToRoot : TinyMessageBase
    {
        public CloseViewsToRoot(object sender)
            : base(sender)
        {
        }

    }
}