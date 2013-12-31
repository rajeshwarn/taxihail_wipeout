using System.Collections.Generic;
using Android.App;
using Android.Content;

using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Localization;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.Client.Localization
{
    public class ResourceManager : IAppResource
    {
        private readonly Context _context;


        public ResourceManager(Context context)
        {
            _context = context;
        }

        public AppLanguage CurrentLanguage
        {
            get
            {
                return Application.Context.GetString(Resource.String.Language) == "English"
                    ? AppLanguage.English
                    : AppLanguage.Francais;
            }
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
            var identifier = _context.Resources.GetIdentifier(key, "string", _context.PackageName);
            return _context.Resources.GetString(identifier);
        }

        public List<TutorialItemModel> GetTutorialItemsList()
        {
            return new List<TutorialItemModel>
            {
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen01",
                    TopText = GetString("Tuto01Top"),
                    BottomText = GetString("Tuto01Bottom")
                },
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen02",
                    TopText = GetString("Tuto02Top"),
                    BottomText = GetString("Tuto02Bottom")
                },
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen03",
                    TopText = GetString("Tuto02Top"),
                    BottomText = GetString("Tuto03Bottom")
                },
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen04",
                    TopText = GetString("Tuto03Top"),
                    BottomText = GetString("Tuto04Bottom")
                },
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen05",
                    TopText = GetString("Tuto04Top"),
                    BottomText = GetString("Tuto05Bottom")
                },
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen06",
                    TopText = GetString("Tuto05Top"),
                    BottomText = GetString("Tuto06Bottom")
                },
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen07",
                    TopText = GetString("Tuto06Top"),
                    BottomText = GetString("Tuto07Bottom")
                },
                new TutorialItemModel
                {
                    ImageUri = "tutorial_screen08",
                    TopText = GetString("Tuto07Top"),
                    BottomText = GetString("Tuto08Bottom")
                }
            };
        }
    }
}