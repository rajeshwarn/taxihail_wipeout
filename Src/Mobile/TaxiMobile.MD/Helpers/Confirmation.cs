using System;
using Android.App;

namespace TaxiMobile.Helpers
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