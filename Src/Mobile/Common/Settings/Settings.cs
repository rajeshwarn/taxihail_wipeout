using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile
{
	public class Config
	{
		static IAppResource _r =  TinyIoCContainer.Current.Resolve<IAppResource>();
		static IAppSettings _appSettings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
		static IConfigurationManager _configManager = TinyIoCContainer.Current.Resolve<IConfigurationManager>();
		
		public class Client
		{
			public static bool? ShowPassengerPhone {
				get{	
					bool result;
					if(bool.TryParse(_configManager.GetSetting("Client.ShowPassengerPhone"), out result))
					{
						return result;
					}
					return null;
				}
			}

			public static bool? ShowPassengerName {
				get{
					bool result;
					if(bool.TryParse(_configManager.GetSetting("Client.ShowPassengerName"), out result))
					{
						return result;
					}
					return null;
				}
			}

			public static bool? ShowPassengerNumber {
				get{
					bool result;
					if(bool.TryParse(_configManager.GetSetting("Client.ShowPassengerNumber"), out result))
					{
						return result;
					}
					return null;
				}
			}
			
			public static bool? ShowEstimate {
				get{
					bool result;
					if(bool.TryParse(_configManager.GetSetting("Client.ShowEstimate"), out result))
					{
						return result;
					}
					return null;
				}
			}


		}


	}
}

