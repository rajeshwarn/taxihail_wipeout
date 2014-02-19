using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Reactive.Linq;
using Android.Text;

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

        public static IObservable<string> OnKeyDown(this EditText text)
        {
            return Observable.FromEventPattern<EventHandler<AfterTextChangedEventArgs>, EventArgs>(
                ev => text.AfterTextChanged += ev,
                ev => text.AfterTextChanged -= ev)
                    .Select(e=>text.Text);        
        }
    }
}