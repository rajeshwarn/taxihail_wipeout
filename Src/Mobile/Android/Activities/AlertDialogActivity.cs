
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
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Activity(Theme = "@android:style/Theme.Dialog")]
    public class AlertDialogActivity : Activity
    {
        private string _title;
        private string _message;
        private string _ownerId;

        private string _positiveButtonTitle;
        private string _negativeButtonTitle;
        private string _neutralButtonTitle;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _title = Intent.GetStringExtra("Title");
            _message = Intent.GetStringExtra("Message");
            _ownerId = Intent.GetStringExtra("OwnerId");
            _positiveButtonTitle = Intent.GetStringExtra("PositiveButtonTitle");
            _negativeButtonTitle = Intent.GetStringExtra("NegativeButtonTitle");
            _neutralButtonTitle = Intent.GetStringExtra("NeutralButtonTitle");
        }

        protected override void OnStart()
        {
            base.OnStart();
            DisplayError();
        }

        private void DisplayError()
        {
            if (_ownerId.HasValue() && _negativeButtonTitle.HasValue() && _positiveButtonTitle.HasValue() && !_neutralButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle, (s, e) => SendMessage(_positiveButtonTitle), _negativeButtonTitle, (s, e) => SendMessage(_negativeButtonTitle));
            }
            else if (_neutralButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle, (s, e) => SendMessage(_positiveButtonTitle), _negativeButtonTitle, (s, e) => SendMessage(_negativeButtonTitle), _neutralButtonTitle, (s, e) => SendMessage(_neutralButtonTitle));
            }
			else if (_positiveButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle, (s, e) => SendMessage(_positiveButtonTitle));
            }
            else
            {
                AlertDialogHelper.ShowAlert(this, _title, _message, () => SendMessage(_positiveButtonTitle));
            }
        }

        private void SendMessage(string buttonTitle)
        {
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new ActivityCompleted(this, buttonTitle, _ownerId));
            Finish();
        }
    }

}