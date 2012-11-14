using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookConfirmationViewModel : BaseViewModel
    {
        public BookConfirmationViewModel (string order)
        {
            Order = JsonSerializer.DeserializeFromString<CreateOrder>( order );
        }

        public CreateOrder Order{get;private set;}

        public IMvxCommand ConfirmOrderCommand
        {
            get
            {

                return new MvxRelayCommand(() => 
                    {
                        RequestClose(this);
                        TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new OrderConfirmed(this, Order ));
                    }); 
            }
        }

        public IMvxCommand CancelOrderCommand
        {
            get
            {

                return new MvxRelayCommand(() => RequestClose(this ));               
            }
        }

    }
}

