using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.App;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Theme = "@android:style/Theme.Dialog")]
    public class AlertDialogActivity : BaseDialogActivity
    {
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
                this.ShowAlert(_title, _message, Finish);
            }
        }
    }
}