using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
    class LinkerPleaseInclude
    {
        private void IncludeLeftBarButtonItem(UIBarButtonItem button)
        {
            button.Clicked += (sender, e) => {};
        }

		public void Include(UISwitch sw)
		{
			sw.On = !sw.On;
			sw.ValueChanged += (sender, args) => { sw.On = false; };
		}
    }
}

