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
using Android.Views.Accessibility;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.mk.booking.mobile.client.controls.widgets.EditTextWithAccessibility")]
	public class EditTextWithAccessibility:EditText
	{
		public EditTextWithAccessibility(Context context):base(context)
		{
		}

		public EditTextWithAccessibility(Context context, IAttributeSet attrs):base(context, attrs)
		{
		}

		protected EditTextWithAccessibility(IntPtr javaReference, JniHandleOwnership transfer):base(javaReference, transfer)
		{
		}

		public EditTextWithAccessibility(Context context, IAttributeSet attrs, int defStyleAttr):base(context, attrs, defStyleAttr)
		{
		}

		public EditTextWithAccessibility(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes):base(context, attrs, defStyleAttr, defStyleRes)
		{
		}

		public override void OnInitializeAccessibilityNodeInfo(AccessibilityNodeInfo info)
		{
			base.OnInitializeAccessibilityNodeInfo(info);
			info.Text = info.ContentDescription.ToSafeString() + " " + info.Text;
		}
	}
}