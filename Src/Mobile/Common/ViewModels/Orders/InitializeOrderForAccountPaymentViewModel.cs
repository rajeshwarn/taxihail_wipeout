using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using ServiceStack.Text;
using Cirrious.MvvmCross.Plugins.PhoneCall;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class InitializeOrderForAccountPaymentViewModel : PageViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IMvxPhoneCallTask _phone;

		public InitializeOrderForAccountPaymentViewModel(IOrderWorkflowService orderWorkflowService, IMvxPhoneCallTask phone)
		{
			_orderWorkflowService = orderWorkflowService;
			_phone = phone;
		}

		public async void Init()
		{
			var questions = await _orderWorkflowService.GetAccountPaymentQuestions ();
			questions [0].MaxLength = 24;
			Questions = questions.Select(q => new AccountChargeQuestionViewModel(q)).ToList ();

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

								ShowViewModelAndRemoveFromHistory<BookingStatusViewModel>(new
							{
								order = result.Item1.ToJson(),
								orderStatus = result.Item2.ToJson()
							});
						}
					}
					catch(OrderCreationException e)
					{
						Logger.LogError(e);

						var settings = this.Services().Settings;
						var title = this.Services().Localize["ErrorCreatingOrderTitle"];

						if (!Settings.HideCallDispatchButton)
						{
							this.Services().Message.ShowMessage(title,
								e.Message,
								"Call",
								() => _phone.MakePhoneCall (settings.ApplicationName, settings.DefaultPhoneNumber),
								"Cancel",
								delegate { });
						}
						else
						{
							this.Services().Message.ShowMessage(title, e.MessageNoCall);
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

