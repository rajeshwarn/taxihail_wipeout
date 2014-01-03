using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Localization;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppResource
	{
		AppLanguage CurrentLanguage {get;}
		string CurrentLanguageCode {get;}
		string OrderNote{get;}
		string MobileUser{get;}
		string PaiementType{get;}
		string Notes{get;}
// ReSharper disable once InconsistentNaming
		string OrderNoteGPSApproximate{get;}
		string StatusInvalid{get;}
		string CarAssigned{get;}
        string GetString(string key);
	    List<TutorialItemModel> GetTutorialItemsList();
	}
}

