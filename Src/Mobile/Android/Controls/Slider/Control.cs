using Android.Content;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Slider
{
    public class Control : RelativeLayout
    {
        public Control(Context context, LinearLayout layout, int width, int height) : base(context)
        {
            this.SetSize(width, height);

            layout.AddView(this);
        }

        public Control(Context context, int width, int height) : base(context)
        {
            this.SetSize(width, height);
        }

        public void AddViews(params View[] children)
        {
            foreach (var child in children)
            {
                var lp = child.LayoutParameters.AsRelative();

                AddView(child, lp);
            }
        }
    }
}