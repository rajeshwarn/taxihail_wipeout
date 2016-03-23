using System;
using Android.Views.InputMethods;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Behavior
{
    public class NumberAndAddressTextFieldBehavior
    {
        public static void ApplyTo(EditText addressTextField, EditText numberTextField, Func<Address> currentAddress, Action<string> streetNumberOfAddressUpdated)
        {
            numberTextField.Text = "";

            numberTextField.EditorAction += (s, e) => 
            {
                if(e.ActionId == ImeAction.Done)
                {
                    numberTextField.ClearFocus();
                } 
            };

            numberTextField.FocusChange += (s, e) => 
            {
                if (e.HasFocus)
                {
                    FocusOnNumber(addressTextField, numberTextField, currentAddress);
                    numberTextField.ShowKeyboard();
                }
                else
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
                    numberTextField.HideKeyboard();
                }
            };
        }

        private static void FocusOnNumber(EditText addressTextField, EditText numberTextField, Func<Address> currentAddress)
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
            numberTextField.SelectAll();

            // remove the street number from the other textfield 
            addressTextField.Text = addressTextField.Text.Replace(streetNumber, string.Empty).Trim();
        }
    }
}

