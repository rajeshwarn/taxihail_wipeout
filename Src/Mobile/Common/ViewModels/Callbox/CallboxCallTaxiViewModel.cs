using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxCallTaxiViewModel : BaseCallboxViewModel
    {
        public IMvxCommand CallTaxi
        {
            get
            {
                return this.GetCommand(() => this.MessageService.ShowEditTextDialog(Resources.GetString("BookTaxiTitle"), Resources.GetString("BookTaxiPassengerName"), Resources.GetString("Ok"), s
                                                                                                                                                                                                   =>
                                                                                                                                                                                                       {
                                                                                                                                                                                                           RequestNavigate<CallboxOrderListViewModel>(new { passengerName = s },true, MvxRequestedBy.UserAction);
                                                                                                                                                                                                           Close();
                                                                                                                                                                                                       }));
            }
        }
    }
}