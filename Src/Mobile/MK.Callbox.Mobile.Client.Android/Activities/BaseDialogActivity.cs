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
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Callbox.Mobile.Client.Messages;

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
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new ActivityCompleted(this, buttonTitle, _ownerId));
            Finish();
        }

        protected abstract void Display();
    }

}