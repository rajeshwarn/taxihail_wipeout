using System;
using UIKit;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Behavior
{
    public class NumberAndAddressTextFieldBehavior
    {
        public static void ApplyTo(UITextField addressTextField, UITextField numberTextField, Func<Address> currentAddress, Action<string> streetNumberOfAddressUpdated)
        {
            numberTextField.Text = "";
            numberTextField.EditingDidBegin += (s, e) => FocusOnNumber(addressTextField, numberTextField, currentAddress);
            numberTextField.EditingDidEnd += (sender, e) => 
            {
                var currentAddressValue = currentAddress();
                currentAddressValue.ChangeStreetNumber(numberTextField.Text);

                var newFullAddress = currentAddressValue.DisplayAddress;
                if (streetNumberOfAddressUpdated != null)
                {
                    streetNumberOfAddressUpdated(numberTextField.Text);
                }
                addressTextField.Text = newFullAddress;
                numberTextField.Text = "";
            };
        }

        private static void FocusOnNumber(UITextField addressTextField, UITextField numberTextField, Func<Address> currentAddress)
        {
            if (addressTextField.Text == null)
            {
                return;
            }

            var currentAddressValue = currentAddress();
            var streetNumber = currentAddressValue != null
                ? currentAddressValue.StreetNumber
                : string.Empty;

            int dummy;
            if(!int.TryParse(streetNumber, out dummy)) 
            { 
                return; 
            }

            numberTextField.Text = streetNumber;

            //setting the text here resets the font and unless you change it to another 
            //font before setting the correct font, the change won't be visible
            var originalFont = numberTextField.Font;
            numberTextField.Font = UIFont.SystemFontOfSize(10);
            numberTextField.Font = originalFont;

            numberTextField.SelectAll(numberTextField);

            // remove the street number from the other textfield 
            addressTextField.Text = addressTextField.Text.Replace(streetNumber, string.Empty).Trim();
        }
    }
}

