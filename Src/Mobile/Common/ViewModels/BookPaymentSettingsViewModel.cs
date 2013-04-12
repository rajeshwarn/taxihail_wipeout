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
    public class BookPaymentSettingsViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>, IMvxServiceConsumer<IBookingService>
    {

        public BookPaymentSettingsViewModel (string order)
        {
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);   

            var account = AccountService.CurrentAccount;
            var paymentInformation = new PaymentInformation 
			{
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
					if(PaymentPreferences.SelectedCreditCard == null)
					{
						MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, Str.NoCreditCardSelectedMessage);
						return;
					}

                    Order.Id = Guid.NewGuid ();
                    try {                        

                        MessageService.ShowProgress (true);

                        var paymentSettings = new PaymentSettings
						{
                            PayWithCreditCard = true,
                            CreditCardId = PaymentPreferences.SelectedCreditCardId,
                            TipAmount = PaymentPreferences.IsTipInPercent ? null : PaymentPreferences.TipDouble,
                            TipPercent = PaymentPreferences.IsTipInPercent ? PaymentPreferences.TipDouble : null,
                        };
                        Order.Payment = paymentSettings;

                        var orderInfo = BookingService.CreateOrder (Order);
                        
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
							string err = Str.GetServerErrorCreatingOrder(Order.Settings.ProviderId.HasValue ? Order.Settings.ProviderId.Value : 1);
                            MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, err);
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

