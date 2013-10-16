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
		public static string ErrorCreatingOrderTitle {
			get{				
				return _r.GetString ("ErrorCreatingOrderTitle");
			}
		}
		public static string NoCreditCardSelectedMessage {
			get{				
				return _r.GetString ("NoCreditCardSelected");
			}
		}

		public static string NoAmountSelectedMessage {
			get{				
				return _r.GetString ("NoAmountSelectedMessage");
			}
		}

		public  static string NoOrderId {
			get{				
				return _r.GetString ("NoOrderId");
			}
		}

		public static string CmtTransactionErrorMessage {
			get{				
				return _r.GetString ("CmtTransactionErrorMessage");
			}
		}
		public static string SendMessageErrorMessage {
			get{				
				return _r.GetString ("SendMessageErrorMessage");
			}
		}

		public static string CmtTransactionSuccessMessage {
			get{				
				return _r.GetString ("CmtTransactionSuccessMessage");
			}
		}
		public static string CmtTransactionSuccessTitle {
			get{				
				return _r.GetString ("CmtTransactionSuccessTitle");
			}
		}
		public static string CmtTransactionResendConfirmationButtonText {
			get{				
				return _r.GetString ("CmtTransactionResendConfirmationButtonText");
			}
		}
		
		public static string TaxiServerDownMessage {
			get{				
				return _r.GetString ("TaxiServerDownMessage");
			}
		}


		public static string GetPaymentConfirmationMessageToDriver(string formattedAmount)
		{
			return  string.Format (_r.GetString ("PaymentConfirmationMessageToDriver"), formattedAmount);		
		}



		public static string GetServerErrorCreatingOrder(int phoneNumberId)
		{
            return  string.Format (_r.GetString ("ServiceError_ErrorCreatingOrderMessage"), _appSettings.ApplicationName, _config.GetSetting( "DefaultPhoneNumberDisplay" ));
			
		}

	

	}
    
}
