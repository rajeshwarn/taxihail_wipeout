using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Configuration;
using System.Globalization;
using System.Reactive.Linq;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using apcurium.MK.Common;

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
