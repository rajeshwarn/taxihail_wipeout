using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;

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
	}
}

