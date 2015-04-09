using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Behavior
{
    public class PairingCodeBehavior
    {
        public static void ApplyTo(EditText pairingCodeEditText1, EditText pairingCodeEditText2)
        {
            pairingCodeEditText1.AfterTextChanged += (sender, args) =>
            {
                if (args.Editable.ToString().Length == 3)
                {
                    pairingCodeEditText2.RequestFocus();
                }
            };

            pairingCodeEditText2.AfterTextChanged += (sender, args) =>
            {
                if (args.Editable.ToString().Length == 0)
                {
                    pairingCodeEditText1.RequestFocus();
                }
            };
        }
    }
}