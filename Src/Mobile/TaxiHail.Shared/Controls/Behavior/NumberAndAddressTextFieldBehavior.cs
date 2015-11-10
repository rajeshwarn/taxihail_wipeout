using System;
using Android.Views.InputMethods;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Behavior
{
    public class NumberAndAddressTextFieldBehavior
    {
        public static void ApplyTo(EditText address, EditText number, Action<string> streetNumberOfAddressUpdated)
        {
            number.Text = "";

            number.EditorAction += (s, e) => 
            {
                if(e.ActionId == ImeAction.Done){
                    number.ClearFocus();
                } 
            };

            number.FocusChange += (s, e) => 
            {
                if (e.HasFocus)
                {
                    FocusOnNumber(number, address);
                    number.ShowKeyboard();
                }
                else
                {
                    var newFullAddress = number.Text + " " + address.Text.ToSafeString().Trim();
                    if (streetNumberOfAddressUpdated != null)
                    {
                        streetNumberOfAddressUpdated(number.Text);
                    }
                    address.Text = newFullAddress;
                    number.Text = "";
                    number.HideKeyboard();
                }
            };
        }

        private static void FocusOnNumber(EditText streetNumber, EditText address)
        {
            if (address.Text == null)
            {
                return;
            }

            var splitPoint = address.Text.IndexOf(' ');

            if (splitPoint == -1) { return; }

            var value = address.Text.Substring(0, splitPoint);

            int dummy;
            if(!int.TryParse(value, out dummy)) { return; }

            streetNumber.Text = value;
            streetNumber.SelectAll();

            address.Text = address.Text.Substring(splitPoint).Trim();
        }
    }
}

