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
		public static string ThankYouMessage {
			get{
				return String.Format (_r.GetString ("View_BookingStatus_ThankYouMessage"), _appSettings.ApplicationName);		
			}
		}

		public static string ThankYouTitle {
			get{	
				return _r.GetString ("View_BookingStatus_ThankYouTitle");		
			}
		}

		public static string ReturnBookingScreenMessage {
			get{				
				return _r.GetString ("ReturnBookingScreen");
			}
		}

		public static string GetStatusDescription(string statusText) {
			return string.Format (_r.GetString ("StatusDescription"), statusText);
		}
		
		public static string GetStatusInfoText(string message) {
			return string.Format (_r.GetString ("StatusStatusLabel"), message);
		}
		
		public static string BookPickupLocationEmptyPlaceholder {
			get{				
				return _r.GetString ("BookPickupLocationEmptyPlaceholder");
			}
		}

		public static string HistoryDetailSendReceiptButtonText {
			get{				
				return _r.GetString ("HistoryDetailSendReceiptButton");
			}
		}
		public static string  AddReminderTitle {
			get{				
				return _r.GetString ("AddReminderTitle");
			}
		}
		public static string AddReminderMessage {
			get{				
				return _r.GetString ("AddReminderMessage");
			}
		}

		public static string ReminderTitle {
			get{				
				return string.Format(_r.GetString("ReminderTitle"), _appSettings.ApplicationName);
			}
		}
		public static string GetReminderDetails(string address, DateTime date)
		{		
			return string.Format(_r.GetString("ReminderDetails"),address,CultureProvider.FormatTime (date), CultureProvider.FormatDate(date));
		}


		public static string StatusNewRideButtonText {
			get{				
				return _r.GetString ("StatusNewRideButton");
			}
		}
		
		public static string StatusConfirmNewBooking {
			get{				
				return _r.GetString ("StatusConfirmNewBooking");
			}
		}
		
		public static string CannotCancelOrderTitle {
			get{				
				return _r.GetString ("CannotCancelOrderTitle");
			}
		}
		
		public static string CannotCancelOrderMessage {
			get{				
				return _r.GetString ("CannotCancelOrderMessage");
			}
		}
		
		public static string StatusConfirmCancelRide {
			get{				
				return _r.GetString ("StatusConfirmCancelRide");
			}
		}
		public static string StatusConfirmCancelRideErrorTitle {
			get{				
				return _r.GetString ("StatusConfirmCancelRideErrorTitle");
			}
		}
		public static string StatusConfirmCancelRideError {
			get{				
				return _r.GetString ("StatusConfirmCancelRideError");
			}
		}


	}
    
}
