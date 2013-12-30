using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using System;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
	public class CallboxCallTaxiViewModel : BaseCallboxViewModel
	{
	    public IMvxCommand CallTaxi
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
										RequestNavigate<CallboxOrderListViewModel>(new { passengerName = s },true, MvxRequestedBy.UserAction);                                                        
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