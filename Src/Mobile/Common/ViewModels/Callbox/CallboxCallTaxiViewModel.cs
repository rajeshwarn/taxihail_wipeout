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
				return this.GetCommand (async () =>
				{
				    var localize = this.Services().Localize;

                    var name = await this.Services().Message
                        .ShowPromptDialog(localize["BookTaxiTitle"],localize["BookTaxiPassengerName"]);

				    if (name == null)
				    {
				        return;
				    }

					try
					{
						ShowViewModel<CallboxOrderListViewModel> (new { passengerName = name });
					}
					catch (Exception e)
					{
						Logger.LogError (e);
					}
				});
			}
		}
	}
}