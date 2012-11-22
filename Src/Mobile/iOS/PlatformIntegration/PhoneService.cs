using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhoneService : IPhoneService
    {
        public PhoneService ()
        {
        }

        #region IPhoneService implementation

        public void Call (string phoneNumber)
        {
            var url = new NSUrl ("tel://" + phoneNumber);
            
            if (!UIApplication.SharedApplication.OpenUrl (url)) 
            {             
                var av = new UIAlertView ("Not supported", "Calls are not supported on this device", null, Resources.Close, null);
                av.Show ();
            }
        }

        #endregion
    }
}

