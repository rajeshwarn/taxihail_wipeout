using System;
using Cirrious.MvvmCross.Binding.Android.Views;
using Android.Content;
using Android.Util;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class BindableListView : MvxBindableListView
	{
		public BindableListView ( Context context, IAttributeSet attrs, MvxBindableListAdapter adapter ) : base( context, attrs, adapter )
		{
			Initialize();
		}

		public BindableListView ( Context context, IAttributeSet attrs ) : base( context, attrs )
		{
			Initialize();
		}

		private void Initialize()
		{
			TextFilterEnabled = true;
		}

		private string _searchFilter;
		public string SearchFilter {
			get { return _searchFilter; }
			set {
				_searchFilter = value;
				SetFilter();
			}
		}

		private void SetFilter()
		{
			SetFilterText( SearchFilter );
		}

	}
}

