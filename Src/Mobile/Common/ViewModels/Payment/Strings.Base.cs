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
		static IAppResource _r =  TinyIoCContainer.Current.Resolve<IAppResource>();
		static IAppSettings _appSettings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
        static IConfigurationManager _config = TinyIoCContainer.Current.Resolve<IConfigurationManager> ();


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
