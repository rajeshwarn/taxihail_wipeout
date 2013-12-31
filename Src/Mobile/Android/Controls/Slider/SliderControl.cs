using Android.Content;
using Android.Widget;


namespace apcurium.MK.Booking.Mobile.Client.Controls.Slider
{
    public class SliderControl : Control
    {
        public SliderControl(Context context, LinearLayout layout, int width) : base(context, layout, width, 60)
        {
            var yellowBarControl = new ProgressBarControl(context, Resource.Drawable.sliderYellowBar,
                Resource.Drawable.sliderYellowBarBody, default(int), width);
            yellowBarControl.Resize(60);
            
        }
    }
}