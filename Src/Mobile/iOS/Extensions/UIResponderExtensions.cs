using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class UIResponderExtensions
    {
        public static UIViewController FindViewController (this UIResponder responder)
        {
            return (UIViewController)FindViewControllerInternal(responder);
        }

        private static UIResponder FindViewControllerInternal (UIResponder responder)
        {
            if (responder is UIViewController) {
                return responder;
            }
            if (responder is UIView) {
                return FindViewController(responder.NextResponder);
            }
            return null;
        }
    }
}

