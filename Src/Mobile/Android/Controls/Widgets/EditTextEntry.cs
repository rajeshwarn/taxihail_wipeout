using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

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
	}
}