using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
	public class CreateOrderInfo 
	{
		public string PassengerName { get; set; }
		public bool IsPendingCreation { get; set; }
		public CallboxOrderViewModel Order { get; set; }
	}

	public class CallboxOrderListViewModel : BaseCallboxViewModel
	{
        private readonly IBookingService _bookingService;
        private readonly IAccountService _accountService;

        public CallboxOrderListViewModel(IBookingService bookingService,
            IAccountService accountService)
		{
            _bookingService = bookingService;
            _accountService = accountService;
		}

        public void Init(string passengerName)
        {
            Orders = new ObservableCollection<CallboxOrderViewModel>();

            if (!string.IsNullOrEmpty(passengerName))
            {
                _orderToCreate = new CreateOrderInfo { PassengerName = passengerName, IsPendingCreation = true }; 
            }
        }

        private TinyMessageSubscriptionToken _token;
        private bool _isClosed;
        private CreateOrderInfo _orderToCreate;
        private List<Guid> _orderNotified;
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        private ObservableCollection<CallboxOrderViewModel> _orders;

		public ObservableCollection<CallboxOrderViewModel> Orders
		{
			get { return _orders; }
			set
			{
				_orders = value;
				RaisePropertyChanged();
			}
		}

		public string ApplicationName
		{
            get { return Settings.TaxiHail.ApplicationName; }
		}

		private bool _refreshGate = true;

        public override void OnViewLoaded()
		{
            base.OnViewLoaded();
			_isClosed = false;

			_orderNotified = new List<Guid>();

			_serialDisposable.Disposable = ObserveTimerForRefresh()
				.Select(_ => GetActiveOrderStatus())
				.ObserveOn(SynchronizationContext.Current)
				.Where(_ => _refreshGate)
				.Subscribe(orderStatusDetails =>
				{
					_refreshGate = true;
					RefreshOrderStatus(orderStatusDetails);
					_refreshGate = false;
				}, 
				Logger.LogError);                           

			_token = this.Services().MessengerHub.Subscribe<OrderDeleted>(orderId =>
    			{
                    CancelOrder.ExecuteIfPossible(orderId.Content);
    			});

			if (_orderToCreate != null )
			{
				CreateOrder(_orderToCreate.PassengerName).FireAndForget();
			}
		}

		private IObservable<Unit> ObserveTimerForRefresh()
		{
			return Observable
				.Timer(TimeSpan.FromSeconds(2))
				.Select(_ => Unit.Default);
		}

		private void RefreshOrderStatus(IEnumerable<OrderStatusDetail> orderStatus)
		{
			try
			{
				Orders.Clear();

				var orderStatusDetails = orderStatus.ToArray();
				if (_orderToCreate != null
					&& _orderToCreate.Order != null
					&& orderStatusDetails.Any(os => os.OrderId == _orderToCreate.Order.Id))
				{
					_orderToCreate = null;
				}
				else if (_orderToCreate != null
					&& _orderToCreate.Order != null
					&& orderStatusDetails.None(os => os.OrderId == _orderToCreate.Order.Id))
				{
					Orders.Add(_orderToCreate.Order);
				}

				Orders.AddRange(orderStatusDetails.Select(status => new CallboxOrderViewModel(_bookingService)
				{
					OrderStatus = status,
					CreatedDate = status.PickupDate,
					IbsOrderId = status.IBSOrderId,
					Id = status.OrderId
				}));

				if (!Orders.Any() && _orderToCreate == null)
				{
					ShowViewModel<CallboxCallTaxiViewModel>();
					Close();
				}

				if (Orders.None(x => _bookingService.IsCallboxStatusCompleted(x.OrderStatus.IBSStatusId)) && NoMoreTaxiWaiting != null)
				{
					NoMoreTaxiWaiting(this, new EventArgs());

					return;
				}

				var completedOrders = Orders.Where(order => 
					_bookingService.IsCallboxStatusCompleted(order.OrderStatus.IBSStatusId)	&&
					_orderNotified.All(orderId => orderId != order.Id));

				foreach (var order in completedOrders)
				{
					_orderNotified.Add(order.Id);
					if (OrderCompleted != null)
					{
						// TODO: validate this
						OrderCompleted(this, new EventArgs());
					}
				}
			}
			catch (WebServiceException e)
			{
				Logger.LogError(e);
			}
		}

		private OrderStatusDetail[] GetActiveOrderStatus()
		{
			try
			{
				return _accountService.GetActiveOrdersStatus()
					.OrderByDescending(o => o.PickupDate)
					.Where(status => _bookingService.IsCallboxStatusActive(status.IBSStatusId))
					.ToArray();
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex);
				return new OrderStatusDetail[0];
			}
		}


		protected void Close()
		{
			base.Close(this);

			UnsubscribeToken();
		}

		public void UnsubscribeToken()
		{
			_serialDisposable.Disposable = null;
			_token.Dispose();
		}

        public ICommand CallTaxi
		{
			get
			{
				return this.GetCommand(async () => 
				{
					var name = await this.Services ().Message.ShowPromptDialog (
						this.Services ().Localize["BookTaxiTitle"],
						this.Services ().Localize["BookTaxiPassengerName"], 
						() => { return; });

					await CreateOrder(name);
				});
			}
		}

        public ICommand CancelOrder
		{
			get
			{
				return this.GetCommand<Guid>(orderId =>
                    this.Services().Message.ShowMessage(this.Services().Localize["CancelOrderTitle"],
                        this.Services().Localize["CancelOrderMessage"],
                        this.Services().Localize["Yes"], () => 
						{
							this.Services().Message.ShowProgress(true);

							try
							{
                                _bookingService.CancelOrder(orderId);
								RemoveOrderFromList(orderId);
							}
							catch
							{
								Thread.Sleep( 500 );
								try
								{
                                    _bookingService.CancelOrder(orderId);
									RemoveOrderFromList(orderId);
								}
								catch 
								{
                                    this.Services().Message.ShowMessage(this.Services().Localize["ServiceError_ErrorCreatingOrderMessage"], this.Services().Localize["ErrorCancellingOrderTitle"]);
								}
							}
							finally
							{
								this.Services().Message.ShowProgress(false);
							}
                        }, this.Services().Localize["No"], () => { }));
			}
		}

		private void RemoveOrderFromList(Guid orderId)
		{
			var orderToRemove = Orders.FirstOrDefault(o => o.Id.Equals(orderId));

			if (orderToRemove != null
                && _orderToCreate != null
                && _orderToCreate.Order != null
                && orderToRemove.Id == _orderToCreate.Order.Id)
			{
				_orderToCreate = null;
			}

			InvokeOnMainThread ( ()=>
			{
				Orders.Remove(orderToRemove) ;
				if (!Orders.Any())
				{                                                   
					ShowViewModel<CallboxCallTaxiViewModel>();
					Close();
				}
			});
		}

        private async Task CreateOrder(string passengerName)
		{
	        using (this.Services().Message.ShowProgress())
	        {
				if (_orderToCreate == null)
				{
					_orderToCreate = new CreateOrderInfo { PassengerName = passengerName, IsPendingCreation = true };
				}

				try
				{
					var pickupAddress = (await _accountService.GetFavoriteAddresses()).FirstOrDefault();

					var newOrderCreated = new CreateOrder
					{
						Id = Guid.NewGuid(),
						Settings = _accountService.CurrentAccount.Settings,
						PickupAddress = pickupAddress,
						PickupDate = DateTime.Now
					};

					if (!string.IsNullOrEmpty(passengerName))
					{
						newOrderCreated.Note = string.Format(this.Services().Localize["Callbox.passengerName"], passengerName);
						newOrderCreated.Settings.Name = passengerName;
					}
					else
					{
						newOrderCreated.Note = this.Services().Localize["Callbox.noPassengerName"];
						newOrderCreated.Settings.Name = this.Services().Localize["NotSpecified"];
					}
				
					var orderInfo = await _bookingService.CreateOrder(newOrderCreated);

					//TODO: modify to make the UI more reactive, aka we should not be needing to wait for the IbsOrderId.
					//We need to wait for an ibsOrderId
					while (!orderInfo.IBSOrderId.HasValue && VehicleStatuses.CancelStatuses.None(status => status == orderInfo.IBSStatusId))
					{
						await Task.Delay(TimeSpan.FromMilliseconds(200));

						orderInfo = await _bookingService.GetOrderStatusAsync(orderInfo.OrderId);
					}

					var tcs = new TaskCompletionSource<Unit>();

					InvokeOnMainThread(() =>
					{
						if (pickupAddress != null)
						{
							if (orderInfo.IBSOrderId.HasValue && orderInfo.IBSOrderId > 0)
							{
								orderInfo.Name = newOrderCreated.Settings.Name;

								var orderViewModel = new CallboxOrderViewModel(_bookingService)
								{
									CreatedDate = DateTime.Now,
									IbsOrderId = orderInfo.IBSOrderId,
									Id = newOrderCreated.Id,
									OrderStatus = orderInfo
								};

								if (_orderToCreate != null)
								{
									_orderToCreate.Order = orderViewModel;
								}

								Orders.Add(orderViewModel);
							}
						}
						else
						{
							this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], this.Services().Localize["NoPickupAddress"]);
						}

						tcs.TrySetResult(Unit.Default);
					});

					await tcs.Task;
				}
				catch (Exception e)
				{
					Logger.LogError(e);
					InvokeOnMainThread(() =>
					{
						var err = string.Format(this.Services().Localize["ServiceError_ErrorCreatingOrderMessage"], Settings.TaxiHail.ApplicationName, this.Services().Settings.DefaultPhoneNumberDisplay);
						this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], err);
					});
				}
				finally
				{
					_orderToCreate.IsPendingCreation = false;
				}
	        }

			
		}

		public delegate void OrderHandler(object sender,EventArgs args);

		public event OrderHandler OrderCompleted;
		public event OrderHandler NoMoreTaxiWaiting;
	}
}