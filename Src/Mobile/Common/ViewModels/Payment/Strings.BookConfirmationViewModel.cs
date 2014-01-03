namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public partial class Str
	{
		public static string GetServiceErrorCreatingOrderMessage(int phoneNumberId){

            return string.Format (_r.GetString ("ServiceError_ErrorCreatingOrderMessage"), _appSettings.ApplicationName, _config.GetSetting( "DefaultPhoneNumberDisplay" ));
		}
	
		public static string HistoryDetailBuildingNameNotSpecified {
			get{	
				return _r.GetString ("HistoryDetailBuildingNameNotSpecified");		
			}
		}

		public static string WarningTitle {
			get{	
				return _r.GetString ("WarningTitle");		
			}
		}
		public static string WarningEstimateTitle {
			get{	
				return _r.GetString ("WarningEstimateTitle");		
			}
		}
		public static string WarningEstimate {
			get{	
				return _r.GetString ("WarningEstimate");		
			}
		}
		public static string WarningEstimateDontShow {
			get{	
				return _r.GetString ("WarningEstimateDontShow");		
			}
		}

	}
    
}
