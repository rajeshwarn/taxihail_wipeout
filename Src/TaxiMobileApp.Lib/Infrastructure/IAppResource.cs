using System;

namespace TaxiMobileApp
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

