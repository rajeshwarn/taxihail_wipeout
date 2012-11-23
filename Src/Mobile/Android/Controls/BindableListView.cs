using System;
using Cirrious.MvvmCross.Binding.Android.Views;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class BindableListView : MvxBindableListView
	{
		public BindableListView ( Context context, IAttributeSet attrs, MvxBindableListAdapter adapter ) : base( context, attrs, adapter )
		{
			Initialize();
		}

		public BindableListView ( Context context, IAttributeSet attrs ) : base( context, attrs, new MvxFilteringBindableListAdapter( context ) )
		{
			Initialize();
		}

		private void Initialize()
		{
		}
	}
}

