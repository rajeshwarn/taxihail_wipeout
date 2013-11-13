using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using System;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxCallTaxiViewModel : BaseCallboxViewModel
    {
        public override void Load()
        {
            base.Load();
        }

        public IMvxCommand CallTaxi
        {
            get
            {
                return this.GetCommand(() => 
                                       InvokeOnMainThread ( ()=>
                                        {
                                            this.MessageService.ShowEditTextDialog(Resources.GetString("BookTaxiTitle"), 
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