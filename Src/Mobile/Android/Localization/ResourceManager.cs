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
using TaxiMobileApp;

namespace TaxiMobile.Localization
{
    public class ResourceManager : IAppResource
    {
        public AppLanguage CurrentLanguage
        {
            get { return AppContext.Current.App.GetString(Resource.String.Language) == "English" ? AppLanguage.English : AppLanguage.Francais; }
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
            get { return ""; }
        }

        public string PaiementType
        {
            get { return ""; }
        }

        public string CurrentLanguageCode
        {
            get { return ""; }
        }


        public string CarAssigned
        {
            get { return AppContext.Current.App.GetString( Resource.String.CarAssigned ); }
        }
    }
}