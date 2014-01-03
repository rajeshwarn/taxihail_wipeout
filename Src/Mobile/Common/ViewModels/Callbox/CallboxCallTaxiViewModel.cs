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
							this.Services().Message.ShowEditTextDialog(this.Services().Resources.GetString("BookTaxiTitle"), 
								this.Services().Resources.GetString("BookTaxiPassengerName"), this.Services().Resources.GetString("Ok"), 
								s =>
								{ 
									try
									{
                            			RequestClose( this );
										RequestNavigate<CallboxOrderListViewModel>(new { passengerName = s }, true);                                                        
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