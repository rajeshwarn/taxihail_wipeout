using System;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Droid.Views;
using Android.Content;
using Android.App;
using Cirrious.CrossCore.Platform;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter : MvxAndroidViewPresenter
    {
        public PhonePresenter()
        {
        }

        public override void Show(Cirrious.MvvmCross.ViewModels.MvxViewModelRequest request)
        {
            bool removeFromHistory = request.ParameterValues != null
                                     && request.ParameterValues.ContainsKey("removeFromHistory");

            Intent intent = this.CreateIntentForRequest (request);
            this.Show (intent, removeFromHistory);
        }

        private void Show (Intent intent, bool removeFromHistory)
        {
            Activity activity = this.Activity;
            if (activity == null)
            {
                MvxTrace.Warning ("Cannot Resolve current top activity", new object[0]);
                return;
            }
            activity.StartActivity (intent);
            if (removeFromHistory)
            {
                activity.Finish();
            }
        }
    }
}

