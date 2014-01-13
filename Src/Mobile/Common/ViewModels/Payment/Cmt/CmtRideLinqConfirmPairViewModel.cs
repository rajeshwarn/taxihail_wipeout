using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using System.Linq;

namespace apcurium.MK.Booking.Mobile
{
	public class CmtRideLinqConfirmPairViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>, IMvxServiceConsumer<IPaymentService>
	{
		public CmtRideLinqConfirmPairViewModel(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();  
			_paymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), new PaymentInformation
				{
					CreditCardId = AccountService.CurrentAccount.DefaultCreditCard,
					TipPercent = AccountService.CurrentAccount.DefaultTipPercent,
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

		public string _cardNumber = "";
		public string CardNumber
		{
			get
			{
				if (_cardNumber != "")
				{
					return _cardNumber;
				}
				else return "None";
			}
		}

		public string TipAmountInPercent
		{
			get
			{
				var tipAmount = _paymentPreferences.Tip;
				return tipAmount.ToString() + "%";                
			}
		}        

		public IMvxCommand ConfirmPayment
		{
			get {
				return GetCommand (() =>
					{              
						using(MessageService.ShowProgress())
						{
							if (_paymentPreferences.SelectedCreditCard == null)
							{
								MessageService.ShowMessage(Resources.GetString("PaymentErrorTitle"), Str.NoCreditCardSelectedMessage);
								return;
							}

							var pairingResponse = PaymentService.Pair(Order.Id, _paymentPreferences.SelectedCreditCard.Token, _paymentPreferences.Tip, null);                    

							CacheService.Set("CmtRideLinqPairState" + Order.Id.ToString(), pairingResponse.IsSuccessfull ? CmtRideLinqPairingState.Success : CmtRideLinqPairingState.Failed);

							if(!pairingResponse.IsSuccessfull)
							{
								MessageService.ShowMessage(Resources.GetString("PaymentErrorTitle"), Resources.GetString("CmtRideLinqGenericErrorMessage"));
								return;
							}

							RequestClose(this);
						}
					});
			}
		}

		public IMvxCommand ChangePaymentInfo
		{
			get
			{
				return GetCommand(() =>
					{
						RequestSubNavigate<CmtRideLinqChangePaymentViewModel, PaymentInformation>(
							new
							{
								currentPaymentInformation = new PaymentInformation
								{
									CreditCardId = _paymentPreferences.SelectedCreditCardId,
									TipPercent = _paymentPreferences.Tip,
								}.ToJson()
							}.ToStringDictionary(), result =>
							{                                                
								_paymentPreferences.SelectedCreditCardId = (Guid)result.CreditCardId;
								_paymentPreferences.Tip = (int)result.TipPercent;
								FirePropertyChanged(() => TipAmountInPercent);
								RefreshCreditCards();
							});
					});
			}
		}

		public IMvxCommand CancelPayment
		{
			get
			{
				return GetCommand(() =>
					{
						CacheService.Set("CmtRideLinqPairState" + Order.Id.ToString(), CmtRideLinqPairingState.Canceled);

						RequestClose(this);                
					});
			}
		}
	}
}