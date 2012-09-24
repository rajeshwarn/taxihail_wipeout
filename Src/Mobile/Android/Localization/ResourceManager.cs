using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Localization;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Localization
{
    public class ResourceManager : IAppResource
    {
        
        private Context _context;


        public ResourceManager(Context context)
        {
            _context = context;
        }

        public AppLanguage CurrentLanguage
        {
            get { return Application.Context.GetString(Resource.String.Language) == "English" ? AppLanguage.English : AppLanguage.Francais; }
        }

        public string OrderNote
        {
            get { return ""; }
        }

        public string OrderNoteGPSApproximate
        {
            get { return ""; }
        }

        public string StatusInvalid
        {
            get { return ""; }
        }

        public string Notes
        {
            get { return ""; }
        }

        public string MobileUser
        {
            get { return "This booking was made using the mobile application."; }
        }

        public string PaiementType
        {
            get { return ""; }
        }

        public string CurrentLanguageCode
        {
            get { return Application.Context.GetString(Resource.String.LanguageCode); }
        }


        public string CarAssigned
        {
            get { return Application.Context.GetString(Resource.String.CarAssigned); }
        }

        public string GetString(string key)
        {
            var identifier = _context.Resources.GetIdentifier(key,"string", _context.PackageName);
            return _context.Resources.GetString(identifier);
        }

    }
}