using System;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class EditTextSpinnerSelectedItemBinding : MvxAndroidTargetBinding
    {
        private object _currentValue;

		public EditTextSpinnerSelectedItemBinding(EditTextSpinner target)
			:base(target)
        {
			target.ItemSelected += _spinner_ItemSelected;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        public override Type TargetType
        {
            get { return typeof (object); }
        }

        private void _spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
			var control = (EditTextSpinner)Target;
            var spinner = control.GetSpinner();
            var item = spinner.GetItemAtPosition(e.Position).Cast<ListItemData>();

            var newValue = item.Key;
            if (!newValue.Equals(_currentValue))
            {
                _currentValue = newValue;
                FireValueChanged(item == ListItemData.NullListItemData ? default(int?) : newValue);
            }
        }

		protected override void SetValueImpl(object target, object value)
        {
			var control = (EditTextSpinner)target;
            if (value != _currentValue)
            {
                _currentValue = value;
                control.SetSelection((int) value);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
			var control = (EditTextSpinner)Target;
            if (isDisposing)
            {
                control.ItemSelected -= _spinner_ItemSelected;
            }
            base.Dispose(isDisposing);
        }
    }
}