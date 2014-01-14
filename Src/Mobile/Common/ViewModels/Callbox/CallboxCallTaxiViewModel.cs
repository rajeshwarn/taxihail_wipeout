using System;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
	public class CallboxCallTaxiViewModel : BaseCallboxViewModel
	{
	    public AsyncCommand CallTaxi
		{
			get
			{
				return GetCommand(() => 
					InvokeOnMainThread(()=>
						{
                            this.Services().Message.ShowEditTextDialog(this.Services().Localize["BookTaxiTitle"],
                                this.Services().Localize["BookTaxiPassengerName"], this.Services().Localize["Ok"], 
								s =>
								{ 
									try
									{
                            			Close( this );
										ShowViewModel<CallboxOrderListViewModel>(new { passengerName = s });                                                        
									}
									catch( Exception e )
									{
										Logger.LogError( e );
									}
								});
						}));
			}
		}
	}
}