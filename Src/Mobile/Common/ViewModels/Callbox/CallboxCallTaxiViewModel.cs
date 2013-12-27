using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using System;

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
							MessageService.ShowEditTextDialog(Resources.GetString("BookTaxiTitle"), 
								Resources.GetString("BookTaxiPassengerName"), Resources.GetString("Ok"), 
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