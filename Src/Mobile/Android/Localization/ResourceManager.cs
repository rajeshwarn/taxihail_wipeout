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
			get { return Application.Context.GetString(Resource.String.OrderNote); }
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

        public List<TutorialItemModel> GetTutorialItemsList()
        {
            return new List<TutorialItemModel>()
                                   {
                                       new TutorialItemModel() {ImageUri = "tuto01", TopText = "Here's how to book a ride with just a couple of taps.", BottomText = "Start by selecting your current position."},
                                       new TutorialItemModel() {ImageUri = "tuto02",  TopText = "We should be able to locate you using the GPS receiver in your mobile device.", BottomText = "If it's abstract little off, simply reposition the map to your actual location."}/*,
                                       new TutorialItemModel() {ImageUri = "tuto03",  Text = "Page3"}*/
                                   };
        }

    }
}