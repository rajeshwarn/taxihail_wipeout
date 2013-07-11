using System;
using Cirrious.MvvmCross.Binding.Android.Target;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Android;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Interfaces.Platform.Diagnostics;
using Cirrious.MvvmCross.Binding.Interfaces;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class EditTextSpinnerSelectedItemBinding: MvxBaseAndroidTargetBinding
	{
		private readonly EditTextSpinner _control;
		private object _currentValue;
		
		public EditTextSpinnerSelectedItemBinding(EditTextSpinner control)
		{
			_control = control;
			_control.ItemSelected += _spinner_ItemSelected;
		}
		
		void _spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			var spinner = _control.GetSpinner();
			var item =  spinner.GetItemAtPosition (e.Position).Cast<ListItemData>();

			var newValue = item.Key;
			if (!newValue.Equals(_currentValue))
			{
				_currentValue = newValue;
				if (item == ListItemData.NullListItemData) {
					// NullListItemData is selected, so value should be null
					FireValueChanged (default(int?));
				}
				else {
					FireValueChanged(newValue);
				}
			}

		}
		
		public override void SetValue(object value)
		{
			if (!value.Equals(_currentValue))
			{
				_currentValue = value;
				_control.SetSelection((int)value);
			}
		}
		
		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.TwoWay; }
		}
		
		public override Type TargetType
		{
			get { return typeof(object); }
		}
		
		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				_control.ItemSelected -= _spinner_ItemSelected;
			}
			base.Dispose(isDisposing);
		}
	}
}

