using System.Collections.Generic;
using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Localization;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Callbox.Mobile.Client.Localization
{
	public class ResourceManager : ILocalization
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

		public string this[string key]
        {
			get
			{
				var identifier = _context.Resources.GetIdentifier(key, "string", _context.PackageName);
				return _context.Resources.GetString(identifier);
			}
        }

        public List<TutorialItemModel> GetTutorialItemsList()
        {
            return new List<TutorialItemModel>()
			{
				new TutorialItemModel() {ImageUri = "tutorial_screen01", TopText = this["Tuto01Top"], BottomText = this["Tuto01Bottom"]},
				new TutorialItemModel() {ImageUri = "tutorial_screen02",  TopText = this["Tuto02Top"], BottomText = this["Tuto02Bottom"]},
				new TutorialItemModel() {ImageUri = "tutorial_screen03",  TopText = this["Tuto02Top"], BottomText = this["Tuto03Bottom"]},
				new TutorialItemModel() {ImageUri = "tutorial_screen04",  TopText = this["Tuto03Top"], BottomText = this["Tuto04Bottom"]},
				new TutorialItemModel() {ImageUri = "tutorial_screen05",  TopText = this["Tuto04Top"], BottomText = this["Tuto05Bottom"]},
				new TutorialItemModel() {ImageUri = "tutorial_screen06",  TopText = this["Tuto05Top"], BottomText = this["Tuto06Bottom"]},
				new TutorialItemModel() {ImageUri = "tutorial_screen07",  TopText = this["Tuto06Top"], BottomText = this["Tuto07Bottom"]},
				new TutorialItemModel() {ImageUri = "tutorial_screen08",  TopText = this["Tuto07Top"], BottomText = this["Tuto08Bottom"]}
			};
        }

    }
}