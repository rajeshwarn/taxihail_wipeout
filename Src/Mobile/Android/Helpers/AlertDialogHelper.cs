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
using apcurium.MK.Booking.Mobile.Framework.Extensions;


namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class AlertDialogHelper
    {
        public static void ShowAlert(this Activity owner, string title, string message, Action onClose = null)
        {
            Show(owner, title, message, onClose);
        }

        public static void ShowAlert(this Activity owner, int titleResId, int messageResId, Action onClose = null)
        {
            Show(owner, titleResId, messageResId, onClose);
        }

        public static void ShowAlert(this Activity owner, int titleResId, Action onClose = null)
        {
            Show(owner, titleResId, null, onClose);
        }

        public static void Show(Activity owner, int titleResId, int? messageResId, Action onClose = null)
        {
            Show(owner, owner.GetString(titleResId), messageResId.HasValue ? owner.GetString(messageResId.Value) : null, onClose );            
        }

        public static void Show(Activity owner, string title, string message, Action onClose = null)
        {
            var dialog = new AlertDialog.Builder(owner).Create();
            dialog.SetTitle(title);
            if (message.HasValue())
            {
                dialog.SetMessage(message);
            }
            dialog.SetButton("Ok", delegate
            {
                if (onClose != null)
                {
                    onClose();
                }
            });
            dialog.Show();
        }

        public static void Show(Activity owner, string title, string message, string positiveButtonTitle, EventHandler<DialogClickEventArgs> positiveClickHandler)
        {
            var dialog = new AlertDialog.Builder(owner);
            dialog.SetPositiveButton(positiveButtonTitle, positiveClickHandler);
            dialog.SetTitle(title);
            if (message.HasValue())
            {
                dialog.SetMessage(message);
            }
            dialog.Create();
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

        public static void Show(Activity owner, string title, string message, string positiveButtonTitle, EventHandler<DialogClickEventArgs> positiveClickHandler, string negativeButtonTitle, EventHandler<DialogClickEventArgs> negativeClickHandler, string neutralButtonTitle, EventHandler<DialogClickEventArgs> neutralClickHandler)
        {
            var dialog = new AlertDialog.Builder(owner);
            dialog.SetPositiveButton(positiveButtonTitle, positiveClickHandler);
            dialog.SetNegativeButton(negativeButtonTitle, negativeClickHandler);
            dialog.SetNeutralButton(neutralButtonTitle, neutralClickHandler);
            dialog.SetTitle(title);
            if (message.HasValue())
            {
                dialog.SetMessage(message);
            }
            dialog.Create();

            dialog.Show();

        }

		public static void Show(Activity owner, string title, string[] items, EventHandler<DialogClickEventArgs> onItemSelected)
		{
			if (onItemSelected == null) {
				onItemSelected = (s,e) => {};
			}

			var dialog = new AlertDialog.Builder(owner);
			var adapter = new ArrayAdapter<string>(owner, Android.Resource.Layout.SelectDialogItem, items);
			dialog.SetTitle(title);
			dialog.SetAdapter(adapter, onItemSelected);
			dialog.Show();
		}
		
    }
}