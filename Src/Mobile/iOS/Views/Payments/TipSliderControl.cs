using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    [Register("TipSliderControl")]
    public partial class TipSliderControl : UISlider
    {
        public TipSliderControl (IntPtr handle) :  base(handle)
        {

            MinValue = 0;
            MaxValue = 25;


            ValueChanged+= (sender, e) => {
                Value = (int)(Math.Round(Value/5.0)*5); 
            };
        }
        
    }
}

