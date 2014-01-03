using System;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Cirrious.MvvmCross.Binding.Android.Target;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public enum CellItemBindingProperty
    {
        IsBottom = 0,
        IsTop = 1,
    }

    public class CellItemBinding : MvxBaseAndroidTargetBinding
    {
        private readonly ListViewCell2 _control;
        private readonly CellItemBindingProperty _property;

        public CellItemBinding(ListViewCell2 control, CellItemBindingProperty property)
        {
            _control = control;
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

        public override void SetValue(object value)
        {
            if (_property == CellItemBindingProperty.IsBottom)
            {
                _control.IsBottom = (bool) value;
            }
            else if (_property == CellItemBindingProperty.IsTop)
            {
                _control.IsTop = (bool) value;
            }
        }
    }
}