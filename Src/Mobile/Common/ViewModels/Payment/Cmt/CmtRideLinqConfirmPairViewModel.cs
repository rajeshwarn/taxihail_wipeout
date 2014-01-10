using System;
using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
	public class CmtRideLinqConfirmPairViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>, IMvxServiceConsumer<IPaymentService>
	{
		public CmtRideLinqConfirmPairViewModel(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();  
			_paymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), new PaymentInformation
				{
					CreditCardId = this.Services().Account.CurrentAccount.DefaultCreditCard,
                    TipPercent = this.Services().Account.CurrentAccount.DefaultTipPercent,
				});
			_paymentPreferences.CreditCards.CollectionChanged += (sender, e) => RefreshCreditCards();
		}

		private PaymentDetailsViewModel _paymentPreferences;

		Order Order { get; set; }
		OrderStatusDetail OrderStatus { get; set; }

		public void RefreshCreditCards()
		{
			var selectedCard = _paymentPreferences.SelectedCreditCard;

			if (selectedCard != null)
			{
				_cardNumber = selectedCard.CreditCardCompany + " " + selectedCard.Last4Digits;
			}
			else
			{
				_cardNumber = "";
			}

			FirePropertyChanged(() => CardNumber);
		}

		public string CarNumber{
			get{
				return OrderStatus.VehicleNumber;
			}
		}

		private string _cardNumber = "";
		public string CardNumber
		{
			get
			{
			    if (_cardNumber != "")
				{
					return _cardNumber;
				}
			    return "None";
			}
		}

		public string TipAmountInPercent
		{
			get
			{
				var tipAmount = _paymentPreferences.Tip;
				return tipAmount.ToString(CultureInfo.InvariantCulture) + "%";                
			}
		}        

		public IMvxCommand ConfirmPayment
		{
			get {
				return GetCommand (() =>
					{                      
						if (_paymentPreferences.SelectedCreditCard == null)
						{
                            this.Services().Message.ShowMessage(this.Services().Localize["PaymentErrorTitle"], this.Services().Localize["NoCreditCardSelected"]);
							return;
						}

						var pairingResponse = this.Services().Payment.Pair(Order.Id, _paymentPreferences.SelectedCreditCard.Token, _paymentPreferences.Tip, null);                    

						this.Services().Cache.Set("CmtRideLinqPairState" + Order.Id.ToString(), pairingResponse.IsSuccessfull ? CmtRideLinqPairingState.Success : CmtRideLinqPairingState.Failed);

						RequestClose(this);
					});
			}
		}

		public IMvxCommand ChangePaymentInfo
		{
			get
			{
				return GetCommand(() => RequestSubNavigate<CmtRideLinqChangePaymentViewModel, PaymentInformation>(
				    new
				    {
				        currentPaymentInformation = new PaymentInformation
				        {
				            CreditCardId = _paymentPreferences.SelectedCreditCardId,
				            TipPercent = _paymentPreferences.Tip,
				        }.ToJson()
				    }.ToStringDictionary(), result =>
				    {                                                
// ReSharper disable PossibleInvalidOperationException
				        _paymentPreferences.SelectedCreditCardId = (Guid)result.CreditCardId;
				        _paymentPreferences.Tip = (int)result.TipPercent;
// ReSharper restore PossibleInvalidOperationException
				        FirePropertyChanged(() => TipAmountInPercent);
				        RefreshCreditCards();
				    }));
			}
		}

		public IMvxCommand CancelPayment
		{
			get
			{
				return GetCommand(() =>
					{
						this.Services().Cache.Set("CmtRideLinqPairState" + Order.Id, CmtRideLinqPairingState.Canceled);
						RequestClose(this);                
					});
			}
		}
	}
}