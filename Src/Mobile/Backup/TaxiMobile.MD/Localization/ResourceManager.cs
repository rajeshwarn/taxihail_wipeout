using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Localization;

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

        public string CarAssigned
        {
            get { return AppContext.Current.App.GetString( Resource.String.CarAssigned ); }
        }
    }
}