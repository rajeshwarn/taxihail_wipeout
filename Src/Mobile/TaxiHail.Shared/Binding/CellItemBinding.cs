using System;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public enum CellItemBindingProperty
    {
        IsBottom = 0,
        IsTop = 1,
    }

    public class CellItemBinding : MvxAndroidTargetBinding
    {
        private readonly CellItemBindingProperty _property;

		public CellItemBinding(ListViewCell target, CellItemBindingProperty property)
			:base(target)
        {
            _property = property;
        }


        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        public override Type TargetType
        {
            get { return typeof (bool); }
        }

		protected override void SetValueImpl(object target, object value)
        {
			var control = (ListViewCell)target;
            if (_property == CellItemBindingProperty.IsBottom)
            {
                control.IsBottom = (bool) value;
            }
        }
    }
}