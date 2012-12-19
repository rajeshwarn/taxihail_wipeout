using System;
using apcurium.MK.Booking.Mobile.AppServices;
using ServiceStack.Text;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookPaymentSettingsViewModel : BaseViewModel,
    IMvxServiceConsumer<IAccountService>, IMvxServiceConsumer<IBookingService>
    {
        IAccountService _accountService;
        IBookingService _bookingService;

        public BookPaymentSettingsViewModel (string order)
        {
            _accountService = this.GetService<IAccountService>();
            _bookingService = this.GetService<IBookingService>();
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);   

            var account = _accountService.CurrentAccount;
            var paymentInformation = new PaymentInformation {
                CreditCardId = account.DefaultCreditCard,
                TipAmount = account.DefaultTipAmount,
                TipPercent = account.DefaultTipPercent,
            };
            PaymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
        }

        CreateOrder Order { get; set; }

        public PaymentDetailsViewModel PaymentPreferences {
            get;
            private set;
        }

        public IMvxCommand ConfirmOrderCommand
        {
            get
            {
                
                return GetCommand(() => 
                                  {                    

                    Order.Id = Guid.NewGuid ();
                    try {


                        if(PaymentPreferences.SelectedCreditCard == null)
                        {
                            MessageService.ShowMessage (Resources.GetString ("ErrorCreatingOrderTitle"), Resources.GetString ("NoCreditCardSelected"));
                            return;
                        }

                        MessageService.ShowProgress (true);

                        var paymentSettings = new PaymentSettings{
                            PayWithCreditCard = true,
                            CreditCardId = PaymentPreferences.SelectedCreditCardId,
                            TipAmount = PaymentPreferences.IsTipInPercent ? null : PaymentPreferences.TipDouble,
                            TipPercent = PaymentPreferences.IsTipInPercent ? PaymentPreferences.TipDouble : null,
                        };
                        Order.Payment = paymentSettings;

                        var orderInfo = _bookingService.CreateOrder (Order);
                        
                        if (orderInfo.IBSOrderId.HasValue
                            && orderInfo.IBSOrderId > 0) {

                            var orderCreated = new Order { CreatedDate = DateTime.Now, 
                                DropOffAddress = Order.DropOffAddress, 
                                IBSOrderId = orderInfo.IBSOrderId, 
                                Id = Order.Id, 
                                PickupAddress = Order.PickupAddress, 
                                Note = Order.Note, 
                                PickupDate = Order.PickupDate.HasValue ? Order.PickupDate.Value : DateTime.Now, 
                                Settings = Order.Settings
                            };
                            
                            RequestNavigate<BookingStatusViewModel>(new
                                                                    {
                                order = orderCreated.ToJson(),
                                orderStatus = orderInfo.ToJson()
                            }); 
                            Close();
                            MessengerHub.Publish(new OrderConfirmed(this, Order, false ));
                        }       
                        
                    } catch (Exception ex) {
                        InvokeOnMainThread (() =>
                                            {
                            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
                            string err = string.Format (Resources.GetString ("ServiceError_ErrorCreatingOrderMessage"), settings.ApplicationName, settings.PhoneNumberDisplay (Order.Settings.ProviderId.HasValue ? Order.Settings.ProviderId.Value : 1));
                            MessageService.ShowMessage (Resources.GetString ("ErrorCreatingOrderTitle"), err);
                        });
                    } finally {
                        MessageService.ShowProgress(false);
                    } 

                }); 
                
            }
        }

        public IMvxCommand CancelOrderCommand
        {
            get
            {
                return GetCommand(() => 
                                  {
                    Close();
                    MessengerHub.Publish(new OrderConfirmed(this, Order, true ));
                });            
            }
        }
    }
}

