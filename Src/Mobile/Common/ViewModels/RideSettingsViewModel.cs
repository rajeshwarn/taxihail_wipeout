using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;

namespace apcurium.MK.Booking.Mobile
{
	public class RideSettingsViewModel: BaseSubViewModel<BookingSettings>
	{
		public RideSettingsViewModel (string messageId, string bookingSettings)
			:base(messageId)
		{
			this.BookingSettings = JsonSerializer.DeserializeFromString<BookingSettings>(bookingSettings);
		}

		public BookingSettings BookingSettings {
			get;
			private set;
		}

        public IMvxCommand DoneCommand {
            get {
                return new MvxRelayCommand(()=> {
                    this.ReturnResult(BookingSettings);
                });
            }
        }
	}
}

