using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Localization
{
    public class Localize : ILocalization
    {
        private readonly Context _context;

		ILogger _logger;

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
				if (identifier == 0)
				{
					_logger.LogMessage("Resource Key Not Found {0}", identifier);
					return key;
				}
                return _context.Resources.GetString(identifier);
            }
        }

        public bool Exists(string key)
        {
            var identifier = _context.Resources.GetIdentifier(key, "string", _context.PackageName);
            return identifier != 0;
        }

		public string CurrentLanguage
		{
			get { return this["LanguageCode"]; }
		}

		public bool IsRightToLeft
		{
			get { return this["LanguageCode"] == "ar"; }
		}
    }
}