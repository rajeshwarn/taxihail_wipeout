using System;
using MonoTouch.UIKit;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Behavior
{
    public class NumberAndAddressTextFieldBehavior
    {
        public static void ApplyTo(UITextField address, UITextField number, Action<string,string> addressUpdated)
        {
            number.Text = "";
            number.EditingDidBegin += (s, e) => FocusOnNumber(number, address);
            number.EditingDidEnd += (sender, e) => {
                string newFullAddress = number.Text + " " + address.Text.ToSafeString().Trim();
                if ( addressUpdated !=null && number != null && address != null)
                {
                    addressUpdated( number.Text, newFullAddress );
                }
                address.Text = newFullAddress;
                number.Text = "";
            };
        }

        private static void FocusOnNumber(UITextField streetNumber, UITextField address)
        {
            if (address.Text == null)
            {
                return;
            }

            var splitPoint = address.Text.IndexOf(' ');

            if (splitPoint == -1) { return; }

            var value = address.Text.Substring(0, splitPoint);

            int dummy;
            if(!int.TryParse(value,out dummy)) { return; }

            streetNumber.Text = value;
            streetNumber.SelectAll(streetNumber);

            address.Text = address.Text.Substring(splitPoint).Trim();
        }
    }
}

