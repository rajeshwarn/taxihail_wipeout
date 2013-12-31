using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class EditTextExtensions
    {
        public static void HideKeyboard(this EditText thisControl, Activity parentActivity)
        {
            ((InputMethodManager) parentActivity.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow(
                thisControl.WindowToken, 0); //Hide keyboard
        }
    }
}