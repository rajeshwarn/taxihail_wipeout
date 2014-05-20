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
			_questions = questions.Where (x => x.IsEnabled).ToList();
		}

		private List<AccountPaymentQuestion> _questions;
		public List<AccountPaymentQuestion> Questions
		{ 
			get { return _questions; }
			set
			{
				_questions = value;
				RaisePropertyChanged ();
			}
		}

		public string Question1 
		{
			get { return _questions[0].Question; }
			set 
			{ 
				_questions[0].Question = value;
				RaisePropertyChanged ();
			}
		}

		public string Answer1 
		{
			get { return _questions[0].Answer; }
			set 
			{ 
				_questions[0].Answer = value;
				RaisePropertyChanged ();
			}
		}

		public string Question2
		{
			get { return _questions[1].Question; }
			set 
			{ 
				_questions[1].Question = value;
				RaisePropertyChanged ();
			}
		}

		public string Answer2
		{
			get { return _questions[1].Answer; }
			set 
			{ 
				_questions[1].Answer = value;
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
							// validate and save answers first

							var result = await _orderWorkflowService.ConfirmOrder();

							ShowViewModel<BookingStatusViewModel>(new
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

