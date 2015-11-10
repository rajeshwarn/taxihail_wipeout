using System;
using System.Collections.Generic;
using Android.Views.InputMethods;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Behavior
{
    public class TextFieldInHomeSubviewsBehavior
    {
        public static void ApplyTo(IEnumerable<EditText> editTexts, Action onFocus, Action onLostFocus)
        {
            foreach (var editText in editTexts)
            {
                ApplyTo(editText, onFocus, onLostFocus);
            }
        }

        public static void ApplyTo(EditText editText, Action onFocus, Action onLostFocus)
        {
            if (editText == null) 
            {
                return;
            }

            editText.FocusChange += (sender, e) => 
            {
                if (e.HasFocus)
                {
                    editText.ShowKeyboard();
                    onFocus();
                }
                else
                {
                    onLostFocus();
                    editText.HideKeyboard();
                }
            };

            editText.EditorAction += (sender, e) => 
            {
                if(e.ActionId == ImeAction.Done){
                    editText.ClearFocus();
                } 

                e.Handled = false;
            };
        }
    }
}

