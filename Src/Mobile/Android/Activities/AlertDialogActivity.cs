using Android.App;
using Android.OS;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Messages;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(Label = "Alert Dialog", Theme = "@android:style/Theme.Dialog")]
    public class AlertDialogActivity : Activity
    {
        private string _message;
        private string _negativeButtonTitle;
        private string _neutralButtonTitle;
        private string _ownerId;

        private string _positiveButtonTitle;
        private string _title;

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
            if (_ownerId.HasValue() && _negativeButtonTitle.HasValue() && _positiveButtonTitle.HasValue() &&
                !_neutralButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle,
                    (s, e) => SendMessage(_positiveButtonTitle), _negativeButtonTitle,
                    (s, e) => SendMessage(_negativeButtonTitle));
            }
            else if (_neutralButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle,
                    (s, e) => SendMessage(_positiveButtonTitle), _negativeButtonTitle,
                    (s, e) => SendMessage(_negativeButtonTitle), _neutralButtonTitle,
                    (s, e) => SendMessage(_neutralButtonTitle));
            }
            else if (_positiveButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle,
                    (s, e) => SendMessage(_positiveButtonTitle));
            }
            else
            {
                this.ShowAlert(_title, _message, () => SendMessage(_positiveButtonTitle));
            }
        }

        private void SendMessage(string buttonTitle)
        {
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>()
                .Publish(new ActivityCompleted(this, buttonTitle, _ownerId));
            Finish();
        }
    }
}