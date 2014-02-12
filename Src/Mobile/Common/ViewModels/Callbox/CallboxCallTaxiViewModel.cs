using System;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
	public class CallboxCallTaxiViewModel : BaseCallboxViewModel
	{
        public ICommand CallTaxi
		{
			get
			{
				return this.GetCommand(() => 
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