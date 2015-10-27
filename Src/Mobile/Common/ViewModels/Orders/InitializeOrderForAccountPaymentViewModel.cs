using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class InitializeOrderForAccountPaymentViewModel : PageViewModel, ISubViewModel<Tuple<Order, OrderStatusDetail>>
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IPhoneService _phoneService;

		public InitializeOrderForAccountPaymentViewModel(IOrderWorkflowService orderWorkflowService, IPhoneService phoneService)
		{
			_orderWorkflowService = orderWorkflowService;
			_phoneService = phoneService;
		}

		public async void Init()
		{
			var questions = await _orderWorkflowService.GetAccountPaymentQuestions ();
			Questions = questions.Select (q => new AccountChargeQuestionViewModel (q)).ToList ();
		}

		public async override void OnViewStarted (bool firstTime)
		{
			base.OnViewStarted (firstTime);
			// TODO: This call makes the app crash on Android Nexus 7 - 4.4.2. When is it necessary? (Working on a test device without it.)
			RaisePropertyChanged (() => Questions); //needed for Android
		}

		// the use of list is important here for the binding (doesn't seem to work with an array)
		private List<AccountChargeQuestionViewModel> _questions;
		public List<AccountChargeQuestionViewModel> Questions
		{ 
			get { return _questions; }
			set
			{
				_questions = value;
				RaisePropertyChanged ();
			}
		}

	    public ICommand ConfirmOrder
		{
			get
			{
				return this.GetCommand(async () =>
				{
					try
					{
						using(this.Services().Message.ShowProgress())
						{
							var questionValidationResult = _orderWorkflowService.ValidateAndSaveAccountAnswers(Questions.Select(x => x.Model).ToArray());
							if(!questionValidationResult)
							{
								await this.Services().Message.ShowMessage(
									this.Services().Localize["Error_AccountPaymentTitle"], 
									this.Services().Localize["Error_AccountPaymentQuestionRequiredMessage"]);

								return;
							}

							var result = await _orderWorkflowService.ConfirmOrder();

							this.ReturnResult(result);
						}
					}
					catch(OrderCreationException e)
					{
						Logger.LogError(e);

						var title = this.Services().Localize["ErrorCreatingOrderTitle"];

						if (!Settings.HideCallDispatchButton)
						{
							var serviceType = _orderWorkflowService.GetAndObserveServiceType().Take(1).ToTask();

                            this.Services().Message.ShowMessage(title, e.Message,
								this.Services().Localize["CallButton"], () => _phoneService.Call(serviceType.Result == ServiceType.Luxury ? Settings.DefaultPhoneNumberForLuxury : Settings.DefaultPhoneNumber),
                                this.Services().Localize["Cancel"], () => { });
                        }
						else
						{
							this.Services().Message.ShowMessage(title, e.Message);
						}
					}
					catch(Exception e)
					{
						Logger.LogError(e);
					}
				});
			}
		}
	}
}

