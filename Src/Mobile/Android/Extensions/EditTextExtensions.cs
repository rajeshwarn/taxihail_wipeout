using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class EditTextExtensions
    {
        public static void HideKeyboard(this EditText thisControl)
        {
            ((InputMethodManager)thisControl.Context.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow(thisControl.WindowToken,0);
        }

        public static void ShowKeyboard(this EditText thisControl)
        {
            InputMethodManager inputMethodManager = (InputMethodManager)thisControl.Context.GetSystemService(Context.InputMethodService);
            inputMethodManager.ToggleSoftInput(ShowFlags.Implicit, HideSoftInputFlags.ImplicitOnly);
        }
    }
}