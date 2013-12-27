using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public partial class Str
	{
		static readonly IAppResource _r =  TinyIoCContainer.Current.Resolve<IAppResource>();
		static readonly IAppSettings _appSettings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
        static readonly IConfigurationManager _config = TinyIoCContainer.Current.Resolve<IConfigurationManager> ();


		public static string LoadingMessage {
			get{				
				return _r.GetString ("LoadingMessage");
			}
		}

		public static string YesButtonText {
			get{				
				return _r.GetString ("YesButton");
			}
		}

		public static string OkButtonText {
			get{				
				return _r.GetString("OkButtonText");
			}
		}

		public static string NoButtonText {
			get{				
				return _r.GetString("NoButton");
			}
		}
		public static string RateButtonText {
			get{				
				return _r.GetString ("RateBtn");
			}
		}

		public static string CallButtonText {
			get{				
				return _r.GetString ("CallButton");
			}
		}
		public static string CancelButtonText {
			get{				
				return _r.GetString ("CancelBoutton");
			}
		}
		
		public static string ContinueButtonText {
			get{				
				return _r.GetString ("ContinueButton");
			}
		}
	}
    
}
