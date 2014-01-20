using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Localization
{
    public class Localize : ILocalization
    {
        private readonly Context _context;
        private readonly ILogger _logger;

        public Localize(Context context, ILogger logger)
        {
            _logger = logger;
            _context = context;
        }

        public string this[string key]
        {
            get
            {
                var identifier = _context.Resources.GetIdentifier(key, "string", _context.PackageName);
                try
                {
                    return _context.Resources.GetString(identifier);
                }
                catch(Android.Content.Res.Resources.NotFoundException e)
                {
                    _logger.LogMessage("Resource not found: {0}", key);
                    throw;
                }
            }
        }
    }
}