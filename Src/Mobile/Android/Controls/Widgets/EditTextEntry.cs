using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class EditTextEntry : EditText
	{
		public EditTextEntry(Context context)
			: base(context)			
		{
		}

		public EditTextEntry(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
		}

        public EditTextEntry(IntPtr ptr, JniHandleOwnership handle)
			: base(ptr, handle)
		{
		}

        public override bool OnPreDraw ()
        {
            SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);
            return base.OnPreDraw ();
        }
            
        public bool IsMultiline = false;

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (IsMultiline)
            {
                if (e.KeyCode == Keycode.Enter)
                {
                    var curCarPos = SelectionStart;
                    var endCarPos = SelectionEnd;
                    Text = Text.Substring(0, SelectionStart) + System.Environment.NewLine + Text.Substring(endCarPos);
                    SetSelection((curCarPos + 1) < Text.Length ? curCarPos + 1 : Text.Length);
                    return false;
                }
            }
                
            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyPreIme(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
        {
            // intercept the back button to hide the keyboard
            if (e.KeyCode == Keycode.Back)
            {
                ClearFocus();
                this.HideKeyboard();
                return true;
            }

            return base.OnKeyPreIme(keyCode, e);
        }
	}
}