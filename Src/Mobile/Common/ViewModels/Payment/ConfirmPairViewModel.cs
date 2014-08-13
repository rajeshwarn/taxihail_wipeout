using System;
using System.Globalization;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
	public class ConfirmPairViewModel : PageViewModel
	{
		private readonly IPaymentService _paymentService;

		public ConfirmPairViewModel(IPaymentService paymentService)
		{
			_paymentService = paymentService;
		}

		public async void Init(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();  
			_paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
			await _paymentPreferences.Start();

			RefreshCreditCardNumber ();
		}

		private PaymentDetailsViewModel _paymentPreferences;

		Order Order { get; set; }
		OrderStatusDetail OrderStatus { get; set; }

		public void RefreshCreditCardNumber()
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

			RaisePropertyChanged(() => CardNumber);
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

		public ICommand ConfirmPayment
		{
			get {
				return this.GetCommand (async () =>
					{         
						using(this.Services().Message.ShowProgress())
						{   
							if (_paymentPreferences.SelectedCreditCard == null)
							{
								this.Services().Message.ShowMessage(this.Services().Localize["CmtRideLinqErrorTitle"], this.Services().Localize["NoCreditCardSelected"]);
								return;
							}

						var pairingResponse = await _paymentService.Pair(Order.Id, _paymentPreferences.SelectedCreditCard.Token, _paymentPreferences.Tip, null);                    

							this.Services().Cache.Set("PairState" + Order.Id, pairingResponse.IsSuccessfull ? PairingState.Success : PairingState.Failed);

							if(!pairingResponse.IsSuccessfull)
							{
								this.Services().Message.ShowMessage(this.Services().Localize["CmtRideLinqErrorTitle"], this.Services().Localize["CmtRideLinqGenericErrorMessage"]);
								return;
							}

							Close(this);
						}
					});
			}
		}

		public ICommand CancelPayment
		{
			get
			{
				return this.GetCommand(() =>
					{
						this.Services().Cache.Set("PairState" + Order.Id, PairingState.Canceled);
						Close(this);                
					});
			}
		}
	}
}