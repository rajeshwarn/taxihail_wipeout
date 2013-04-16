using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Theme = "@android:style/Theme.Dialog")]
    public class EditTextDialogActivity : BaseDialogActivity
    {
        protected override void Display()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle(_title);
            alert.SetMessage(_message);

            EditText input = new EditText(this);
            alert.SetView(input);

            alert.SetPositiveButton(_positiveButtonTitle, (s, e) => SendMessage(input.Text));

            alert.Show();
        }
    }
}