using Android.App;
using Android.OS;
using TinyIoC;
using TinyMessenger;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Client.Messages;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    
    public abstract class BaseDialogActivity : Activity
    {
        protected string _title;
        protected string _message;
        protected string _ownerId;

        protected string _positiveButtonTitle;
        protected string _negativeButtonTitle;
        protected string _neutralButtonTitle;

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
            Display();
        }

        protected void SendMessage(string buttonTitle)
        {
            Finish();

            Task.Factory.StartNew(() =>
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new ActivityCompleted(this, buttonTitle, _ownerId));
            });

         
        }

        protected abstract void Display();
    }

}