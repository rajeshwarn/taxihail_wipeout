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
using apcurium.Framework.Extensions;


namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class AlertDialogHelper
    {
        public static void ShowAlert(this Activity owner, string title, string message)
        {
            Show(owner, title, message);
        }


        public static void ShowAlert(this Activity owner, int titleResId, int messageResId)
        {
            Show(owner, titleResId, messageResId);
        }

        public static void ShowAlert(this Activity owner, int titleResId)
        {
            Show(owner, titleResId, null);
        }

        public static void Show(Activity owner, int titleResId, int? messageResId)
        {
            Show(owner, owner.GetString(titleResId), messageResId.HasValue ? owner.GetString(messageResId.Value) : null );            
        }

        public static void Show(Activity owner, string title, string message)
        {
            var dialog = new AlertDialog.Builder(owner).Create();
            dialog.SetTitle(title);
            if (message.HasValue())
            {
                dialog.SetMessage(message);
            }
            dialog.SetButton("Ok", delegate
            {

            });
            dialog.Show();
        }

		public static void Show(Activity owner, string title, string message, string positiveButtonTitle, EventHandler<DialogClickEventArgs> positiveClickHandler, string negativeButtonTitle, EventHandler<DialogClickEventArgs> negativeClickHandler )
        {
            var dialog = new AlertDialog.Builder(owner);
			dialog.SetPositiveButton( positiveButtonTitle, positiveClickHandler );
			dialog.SetNegativeButton( negativeButtonTitle, negativeClickHandler );
            dialog.SetTitle(title);
            if (message.HasValue())
            {
                dialog.SetMessage(message);
            }
			dialog.Create();
            dialog.Show();
        }
		
    }
}