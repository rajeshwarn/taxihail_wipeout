using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Localization
{
    public class Localize : ILocalization
    {
        private readonly Context _context;

        public Localize(Context context)
        {
            _context = context;
        }

        public string this[string key]
        {
            get
            {
                var identifier = _context.Resources.GetIdentifier(key, "string", _context.PackageName);
                return _context.Resources.GetString(identifier);
            }
        }
    }
}