using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class EditTextEntry : EditText
	{
        protected bool setTypeFace = true;

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
            if (setTypeFace)
            {
                SetTypeface(Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);
            }

            return base.OnPreDraw ();
        }

        public void SetClickAnywhereToDismiss()
        {
            var container = (View)this.Parent;
            container.Clickable = true;
            container.Click -= ContainerClicked;
            container.Click += ContainerClicked;
        }

        public void ContainerClicked(object sender, EventArgs e)
        {
            if (((View)sender).Id != this.Id)
            {
                ClearFocus();
                this.HideKeyboard();
            }
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