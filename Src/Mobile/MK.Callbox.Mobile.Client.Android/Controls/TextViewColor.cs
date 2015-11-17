using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Callbox.Mobile.Client.Controls
{
	[Register("apcurium.mk.callbox.mobile.client.controls.TextViewColor")]
    public class TextViewColor : TextView
    {
        public Android.Graphics.Color TextColor
        {
            set{SetTextColor(value);}
        }


        protected TextViewColor(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public TextViewColor(Context context) : base(context)
        {
        }

        public TextViewColor(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public TextViewColor(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
        }
    }
}