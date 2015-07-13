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
using Android.Util;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class EditTextEntryWithoutTypeface : EditTextEntry
    {
		public EditTextEntryWithoutTypeface(Context context)
			: base(context)			
		{
            setTypeFace = false;
		}

		public EditTextEntryWithoutTypeface(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
            setTypeFace = false;
		}

        public EditTextEntryWithoutTypeface(IntPtr ptr, JniHandleOwnership handle)
			: base(ptr, handle)
		{
            setTypeFace = false;
		}


        public override bool OnPreDraw()
        {
            return base.OnPreDraw();
        }
    }
}