using System;
using Android.App;
using Android.Content;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using Android.Views;
using Android.Animation;
using apcurium.MK.Booking.Mobile.Client.Controls.Dialog;
using System.Threading.Tasks;

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
            Show(owner, owner.GetString(titleResId), messageResId.HasValue ? owner.GetString(messageResId.Value) : null,
                onClose);
        }

        public static void Show(Activity owner, string title, string message, Action onClose = null)
        {
            new CustomAlertDialog(owner, title, message, onClose);
        }

        public static void Show(Activity owner, string title, string message, string positiveButtonTitle,
            Action positiveAction)
        {
            new CustomAlertDialog(owner, title, message, positiveAction, positiveButtonTitle);
        }

        public static void Show(Activity owner, string title, string message, string positiveButtonTitle,
            Action positiveAction, string negativeButtonTitle,
            Action negativeAction)
        {

            new CustomAlertDialog(owner, title, message, positiveButtonTitle, positiveAction, negativeButtonTitle, negativeAction);
        }

        public static void Show(Activity owner, string title, string message, string positiveButtonTitle,
            Action positiveAction, string negativeButtonTitle,
            Action negativeAction, string neutralButtonTitle,
            Action neutralAction)
        {
            new CustomAlertDialog(owner, title, message, positiveButtonTitle, positiveAction, 
                negativeButtonTitle, negativeAction, neutralButtonTitle, neutralAction);
        }

        public static void Show(Activity owner, string title, string[] items,
            EventHandler<DialogClickEventArgs> onItemSelected)
        {
            if (onItemSelected == null)
            {
                onItemSelected = (s, e) => { };
            }

            var dialog = new AlertDialog.Builder(owner);
            var adapter = new ArrayAdapter<string>(owner, Android.Resource.Layout.SelectDialogItem, items);
            dialog.SetTitle(title);
            owner.RunOnUiThread(() =>
            {
                dialog.SetAdapter(adapter, onItemSelected);
                dialog.Show();
            });
            
        }
        public static Task<string> ShowPromptDialog(Activity owner, string title, string message, Action cancelAction, bool isNumericOnly = false, string inputText = "")
        {
            var cad = new CustomAlertDialog();
            return cad.ShowPrompt(owner, title, message, cancelAction, isNumericOnly, inputText);
        }
    }
}