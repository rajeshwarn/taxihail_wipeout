using System;
using System.Globalization;
using System.Windows.Input;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
	public class CmtRideLinqConfirmPairViewModel : BaseViewModel
	{
		IAccountService _accountService;
		IPaymentService _paymentService;

		public CmtRideLinqConfirmPairViewModel(IAccountService accountService,
			IPaymentService paymentService)
		{
			_accountService = accountService;	
			_paymentService = paymentService;
		}

		public void Init(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();  
			_paymentPreferences = new PaymentDetailsViewModel();
			_paymentPreferences.Init(new PaymentInformation
				{
				CreditCardId = _accountService.CurrentAccount.DefaultCreditCard,
				TipPercent = _accountService.CurrentAccount.DefaultTipPercent,
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

							this.Services().Cache.Set("CmtRideLinqPairState" + Order.Id.ToString(), pairingResponse.IsSuccessfull ? CmtRideLinqPairingState.Success : CmtRideLinqPairingState.Failed);

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

		public ICommand ChangePaymentInfo
		{
			get
			{
				return this.GetCommand(() => ShowSubViewModel<CmtRideLinqChangePaymentViewModel, PaymentInformation>(
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
						RaisePropertyChanged(() => TipAmountInPercent);
				        RefreshCreditCards();
				    }));
			}
		}

		public ICommand CancelPayment
		{
			get
			{
				return this.GetCommand(() =>
					{
						this.Services().Cache.Set("CmtRideLinqPairState" + Order.Id, CmtRideLinqPairingState.Canceled);
						Close(this);                
					});
			}
		}
	}
}