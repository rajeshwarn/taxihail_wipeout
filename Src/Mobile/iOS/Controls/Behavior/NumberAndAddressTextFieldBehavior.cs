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
                var newFullAddress = number.Text + " " + address.Text.ToSafeString().Trim();
                if ( addressUpdated != null)
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

            //setting the text here resets the font and unless you change it to another 
            //font before setting the correct font, the change won't be visible
            var originalFont = streetNumber.Font;
            streetNumber.Font = UIFont.SystemFontOfSize(10);
            streetNumber.Font = originalFont;

            streetNumber.SelectAll(streetNumber);

            address.Text = address.Text.Substring(splitPoint).Trim();
        }
    }
}

