using Android.App;
using Android.OS;
using apcurium.MK.Callbox.Mobile.Client.Helpers;
using apcurium.MK.Callbox.Mobile.Client.Messages;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using TinyMessenger;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Theme = "@android:style/Theme.Dialog")]
    public class AlertDialogActivity : BaseDialogActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

        protected override void Display()
        {
            if (_ownerId.HasValue() && _negativeButtonTitle.HasValue() && _positiveButtonTitle.HasValue() && !_neutralButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle, (s, e) => SendMessage(_positiveButtonTitle), _negativeButtonTitle, (s, e) => SendMessage(_negativeButtonTitle));
            }
            else if (_neutralButtonTitle.HasValue())
            {
                AlertDialogHelper.Show(this, _title, _message, _positiveButtonTitle, (s, e) => SendMessage(_positiveButtonTitle), _negativeButtonTitle, (s, e) => SendMessage(_negativeButtonTitle), _neutralButtonTitle, (s, e) => SendMessage(_neutralButtonTitle));
            }
            else
            {
                AlertDialogHelper.ShowAlert(this, _title, _message, () => Finish());
            }
        }
    }
}