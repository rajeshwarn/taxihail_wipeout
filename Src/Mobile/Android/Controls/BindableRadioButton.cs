
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
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class BindableRadioButton : RadioButton
	{
		protected BindableRadioButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			Initialize();
		}
		
		private void Initialize()
		{
			this.Click += delegate
			{
				Selected = !Selected;
				if (Selected
				    && SelectedChangedCommand != null
				    && SelectedChangedCommand.CanExecute())
				{
					SelectedChangedCommand.Execute(this.Tag);
				}
			};		
		}
		
		public BindableRadioButton(Context context) : base(context)
		{
			Initialize();
		}
		
		public BindableRadioButton(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Initialize();
		}
		
		public BindableRadioButton(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			Initialize();
		}
		
		public IMvxCommand SelectedChangedCommand { get; set; }
	}
}

