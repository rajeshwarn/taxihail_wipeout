using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
	public class CallboxCallTaxiViewModel : BaseCallboxViewModel
	{
        public ICommand CallTaxi
		{
			get
			{
				return this.GetCommand (() =>
				{
					InvokeOnMainThread (async () =>
					{
						var name = await this.Services ().Message.ShowPromptDialog (
					           this.Services ().Localize["BookTaxiTitle"],
					           this.Services ().Localize["BookTaxiPassengerName"], 
							   () => { return; });

						try
						{
							Close (this);
							ShowViewModel<CallboxOrderListViewModel> (new { passengerName = name });
						}
						catch (Exception e)
						{
							Logger.LogError (e);
						}
					});
				});
			}
		}
	}
}