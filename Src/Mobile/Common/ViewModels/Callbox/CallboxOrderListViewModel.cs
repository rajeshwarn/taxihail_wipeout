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
	public class CallboxOrderListViewModel : BaseCallboxViewModel
	{
        private readonly IBookingService _bookingService;
        private readonly IAccountService _accountService;

        private TinyMessageSubscriptionToken _token;
        private CreateOrderInfo _orderToCreate;
        private List<Guid> _orderNotified;
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        private ObservableCollection<CallboxOrderViewModel> _orders;

        public CallboxOrderListViewModel(IBookingService bookingService, IAccountService accountService)
		{
            _bookingService = bookingService;
            _accountService = accountService;
		}

        public void Init(string passengerName)
        {
            Orders = new ObservableCollection<CallboxOrderViewModel>();

	        var name = passengerName.HasValueTrimmed()
		        ? passengerName
		        : this.Services().Localize["NotSpecified"];

			_orderToCreate = new CreateOrderInfo { PassengerName = name, IsPendingCreation = true }; 
        }

		public ObservableCollection<CallboxOrderViewModel> Orders
		{
			get { return _orders; }
			set
			{
				_orders = value;
				RaisePropertyChanged();
			}
		}

		private bool _refreshGate = true;

        public override void OnViewLoaded()
		{
            base.OnViewLoaded();
			_orderNotified = new List<Guid>();

			_serialDisposable.Disposable = ObserveTimerForRefresh()
				.SelectMany(_ => GetActiveOrderStatus())
				.ObserveOn(SynchronizationContext.Current)
				.Where(_ => _refreshGate)
				.Subscribe(orderStatusDetails =>
				{
					_refreshGate = false;
					RefreshOrderStatus(orderStatusDetails);
					_refreshGate = true;
				},
				ex =>
				{
				    Logger.LogError(ex);
				});                           

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
				.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2))
				.Select(_ => Unit.Default);
		}

		private void RefreshOrderStatus(IEnumerable<OrderStatusDetail> orderStatus)
		{
			try
			{
				Orders.Clear();

				var orderStatusDetails = orderStatus.ToArray();

			    var hasAnOrderToCreate = _orderToCreate != null && _orderToCreate.Order != null;

                if (hasAnOrderToCreate && orderStatusDetails.Any(os => os.OrderId == _orderToCreate.Order.Id))
				{
					_orderToCreate = null;
				}
				else if (hasAnOrderToCreate && orderStatusDetails.None(os => os.OrderId == _orderToCreate.Order.Id))
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

				if (!Orders.Any() && hasAnOrderToCreate)
				{
					Close();

                    return;
				}

				if (Orders.None(x => _bookingService.IsCallboxStatusCompleted(x.OrderStatus.IBSStatusId)) && NoMoreTaxiWaiting != null)
				{
					NoMoreTaxiWaiting(this, new EventArgs());

					return;
				}

				var completedOrders = Orders.Where(order =>
					_bookingService.IsCallboxStatusCompleted(order.OrderStatus.IBSStatusId) &&
					_orderNotified.All(orderId => orderId != order.Id));

				foreach (var order in completedOrders)
				{
					_orderNotified.Add(order.Id);
					if (OrderCompleted != null)
					{
						OrderCompleted(this, new EventArgs());
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError(e);
			}
		}

		private async Task<OrderStatusDetail[]> GetActiveOrderStatus()
		{
            var activeOrders = await _accountService.GetActiveOrdersStatus();

		    if (activeOrders == null)
		    {
		        return new OrderStatusDetail[0];
		    }

            return activeOrders
                .OrderByDescending(o => o.PickupDate)
                .Where(status => _bookingService.IsCallboxStatusActive(status.IBSStatusId))
                .ToArray();
        }


		protected void Close()
		{
            Close(this);

			Unsubscribe();
		}

		public void Unsubscribe()
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
						this.Services ().Localize["BookTaxiPassengerName"]);

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

                            CancelOrderOnServer(orderId).ContinueWith(result => this.Services().Message.ShowProgress(false))
                                .FireAndForget();
                        }, 
                        this.Services().Localize["No"], () => { }));
			}
		}

	    private async Task CancelOrderOnServer(Guid orderId)
	    {
            try
            {
                await _bookingService.CancelOrder(orderId);
                RemoveOrderFromList(orderId);
            }
            catch
            {
                try
                {
                    await Task.Delay(500);
                    await _bookingService.CancelOrder(orderId);
                    RemoveOrderFromList(orderId);
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex);
                    this.Services().Message
                        .ShowMessage(this.Services().Localize["ServiceError_ErrorCreatingOrderMessage"], this.Services().Localize["ErrorCancellingOrderTitle"])
                        .FireAndForget();
                }
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

			InvokeOnMainThread(()=>
			{
				Orders.Remove(orderToRemove) ;

			    if (Orders.Any())
			    {
			        return;
			    }

			    Close();
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

					var newOrderCreated = new CreateOrderRequest
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

					await InvokeOnMainThreadAsync(() =>
					{
					    if (pickupAddress == null)
					    {
                            this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], this.Services().Localize["NoPickupAddress"]);

					        return;
					    }
                        
					    if (orderInfo.IBSOrderId.GetValueOrDefault(0) == 0)
					    {
					        return;
					    }

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
					});
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