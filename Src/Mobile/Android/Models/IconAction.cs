using Android.Content;
using Android.Graphics.Drawables;

namespace apcurium.MK.Booking.Mobile.Client.Models
{
    public class IconAction
    {
        public string ImageUri { get; set; }
        public Intent IntentAction { get; set; }
		public int? RequestCode { get; set; }

        public IconAction()
        {
			ImageUri = null;
            IntentAction = null;
			RequestCode = null;
        }

        public IconAction(string image, Intent intent, int? requestCode)
        {
            ImageUri = image;
            IntentAction = intent;
			RequestCode = requestCode;
        }
    }
}