using TaxiMobile.Lib.Localization;

namespace TaxiMobile.Lib.Infrastructure
{
	public interface IAppResource
	{
		AppLanguage CurrentLanguage {get;}
		
		string OrderNote{get;}
		string OrderNoteGPSApproximate{get;}
		string StatusInvalid{get;}
		string CarAssigned{get;}
		
	}
}

