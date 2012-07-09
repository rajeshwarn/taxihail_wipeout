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

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public class Confirmation
    {
        public void Action(Activity owner, int messageId, Action action)
        {

            AlertDialogHelper.Show(owner, "", owner.GetString(messageId), owner.GetString(Resource.String.YesButton), (IntentSender, args) => 
            {
                var dialog = (AlertDialog)IntentSender;
                dialog.Dismiss();
                action();

            },  
            owner.GetString(Resource.String.NoButton), (IntentSender, args) => 
            {
                var dialog = (AlertDialog)IntentSender;
                dialog.Dismiss();

            } );
            
        }
    }
}