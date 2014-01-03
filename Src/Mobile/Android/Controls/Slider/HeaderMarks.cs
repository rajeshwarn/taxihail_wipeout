using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Slider
{
    public class HeaderMarks : Control
    {
        public HeaderMarks(Context c, LinearLayout layout, int width) : base(c, layout, width, 60)
        {
            var x = new View(Context);
            x.SetSize(50, 60);
            x.SetSize(50, 60);
            x.SetBackgroundColor(Color.White);
        }
    }
}