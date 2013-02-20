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
    public enum CellItemBindingProperty
    {
        IsBottom = 0,
        IsTop = 1,
      
    }

    public class CellItemBinding: MvxBaseAndroidTargetBinding
	{
        private readonly ListViewCell2 _control;
        private CellItemBindingProperty _property;
		private object _currentValue;
		
        public CellItemBinding(ListViewCell2 control, CellItemBindingProperty property)
		{
			_control = control;
            _property = property;
		}
		
		
		public override void SetValue(object value)
		{
			
                if ( _property == CellItemBindingProperty.IsBottom )
                {
                    _control.IsBottom = (bool)value;				
                }
                else if ( _property == CellItemBindingProperty.IsTop )
                {
                    _control.IsTop = (bool)value;                
                }			
        }
		
		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.TwoWay; }
		}
		
		public override Type TargetType
		{
			get { return typeof(bool); }
		}
		
		
	}
}

