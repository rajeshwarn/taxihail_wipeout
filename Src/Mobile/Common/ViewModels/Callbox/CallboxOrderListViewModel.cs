using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using ServiceStack.ServiceClient.Web;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Threading.Tasks;
using System.Threading;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Windows.Input;

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
		private TinyMessageSubscriptionToken _token;

		private bool _isClosed;

		private CreateOrderInfo _orderToCreate;

		private List<int?> _orderNotified;

		private IDisposable _refreshTimer;

		private ObservableCollection<CallboxOrderViewModel> _orders;

		public CallboxOrderListViewModel()
		{
			Orders = new ObservableCollection<CallboxOrderViewModel>();           
			_orderToCreate = null;
		}

		public CallboxOrderListViewModel(string passengerName) : this()
		{
			_orderToCreate = new CreateOrderInfo { PassengerName = passengerName, IsPendingCreation = true }; 
		}

		public ObservableCollection<CallboxOrderViewModel> Orders
		{
			get { return _orders; }
			set { _orders = value; }
		}

		public string ApplicationName
		{
            get { return Settings.ApplicationName; }
		}

        public override void OnViewLoaded()
		{
            base.OnViewLoaded();
			_isClosed = false;

			_orderNotified = new List<int?>();

			_refreshTimer = Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(a => RefreshOrderStatus());                           

			_token = this.Services().MessengerHub.Subscribe<OrderDeleted>(orderId =>
				{
					CancelOrder.Execute(orderId.Content);
				});

			if (_orderToCreate != null )
			{
				CreateOrder(_orderToCreate.PassengerName);
			}
		}

		private void RefreshOrderStatus()
		{
			try
			{
				if ( _isClosed )
				{
					return;
				}

                var orderStatus = this.Services().Account.GetActiveOrdersStatus().ToList().OrderByDescending(o => o.IBSOrderId).Where(status => this.Services().Booking.IsCallboxStatusActive(status.IBSStatusId));
				InvokeOnMainThread(() =>
					{
						Orders.Clear();

					    var orderStatusDetails = orderStatus as OrderStatusDetail[] ?? orderStatus.ToArray();
					    if ( ( _orderToCreate != null ) && ( _orderToCreate.Order != null ) && orderStatusDetails.Any( os=>os.IBSOrderId == _orderToCreate.Order.IBSOrderId ) )
						{
							_orderToCreate = null;
						}
						else if (  ( _orderToCreate != null ) && ( _orderToCreate.Order != null ) && orderStatusDetails.None( os=>os.IBSOrderId == _orderToCreate.Order.IBSOrderId ) )
						{
							Orders.Add(_orderToCreate.Order);
						}

						Orders.AddRange(orderStatusDetails.Select(status => new CallboxOrderViewModel
							{
								OrderStatus = status,
								CreatedDate = status.PickupDate,
								IBSOrderId = status.IBSOrderId,
								Id = status.OrderId
							}));

						if (  (!Orders.Any()) && ( _orderToCreate == null ) ) 
						{
							ShowViewModel<CallboxCallTaxiViewModel>();
							Close();
						}

						if(Orders.None(x => this.Services().Booking.IsCallboxStatusCompleted(x.OrderStatus.IBSStatusId)))
						{
							NoMoreTaxiWaiting(this, null);
						}
						else
						{
							foreach (var order in Orders)
							{
								if (this.Services().Booking.IsCallboxStatusCompleted(order.OrderStatus.IBSStatusId) && !_orderNotified.Any(c => c != null && c.Value.Equals(order.IBSOrderId)))
								{
									_orderNotified.Add(order.IBSOrderId);
									OrderCompleted(this, null);
								}
							}
						}
					});
			}
			catch (WebServiceException e)
			{
				Logger.LogError(e);
			}
			finally
			{
				if (!_isClosed)
				{
					Observable.Timer(TimeSpan.FromSeconds( _orderToCreate == null ? 10 : 2 ))
						.Subscribe(a => RefreshOrderStatus());                           
				}
			}
		}


		protected void Close()
		{
			base.Close(this);

			UnsubscribeToken();
		}

		public void UnsubscribeToken()
		{
			_refreshTimer.Dispose();
			_token.Dispose();
			_isClosed = true;
		}

        public ICommand CallTaxi
		{
			get
			{
				return this.GetCommand(() => this.Services().Message.ShowEditTextDialog(this.Services().Localize["BookTaxiTitle"],
                    this.Services().Localize["BookTaxiPassengerName"],
                    this.Services().Localize["Ok"], 
					CreateOrder));
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
								this.Services().Booking.CancelOrder(orderId);
								RemoveOrderFromList(orderId);
							}
							catch
							{
								Thread.Sleep( 500 );
								try
								{
									this.Services().Booking.CancelOrder(orderId);
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

			if (orderToRemove != null && ((_orderToCreate != null) && ( _orderToCreate.Order != null ) && (orderToRemove.IBSOrderId == _orderToCreate.Order.IBSOrderId )))
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

		private void CreateOrder(string passengerName)
		{        
			this.Services().Message.ShowProgress(true);

			if ( _orderToCreate == null )
			{
				_orderToCreate = new CreateOrderInfo { PassengerName = passengerName, IsPendingCreation = true }; 
			}

			Task.Factory.StartNew(() =>
				{
					try
					{
						var pickupAddress = this.Services().Account.GetFavoriteAddresses().FirstOrDefault();

						var newOrderCreated = new CreateOrder();
						newOrderCreated.Id = Guid.NewGuid();
						newOrderCreated.Settings = this.Services().Account.CurrentAccount.Settings;
						newOrderCreated.PickupAddress = pickupAddress;
						newOrderCreated.PickupDate = DateTime.Now;

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
                        // TODO: Refactor to async/await
                        var orderInfoTask = this.Services().Booking.CreateOrder(newOrderCreated);
                        orderInfoTask.Wait();
                        var orderInfo = orderInfoTask.Result;

						InvokeOnMainThread (() =>
							{                    
								if (pickupAddress != null)
								{
									if (orderInfo.IBSOrderId.HasValue && orderInfo.IBSOrderId > 0)
									{
										orderInfo.Name = newOrderCreated.Settings.Name;
										var o = new CallboxOrderViewModel
										{
											CreatedDate = DateTime.Now,
											IBSOrderId = orderInfo.IBSOrderId,
											Id = newOrderCreated.Id,
											OrderStatus = orderInfo                                            
										};

										if ( _orderToCreate != null )
										{
											_orderToCreate.Order  = o;
										}

										Orders.Add(o);
									}
								}
								else
								{
                                    this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], this.Services().Localize["NoPickupAddress"]);
								}
							});
					}
					catch
					{
						InvokeOnMainThread(() =>
							{
                                string err = string.Format(this.Services().Localize["ServiceError_ErrorCreatingOrderMessage"], Settings.ApplicationName, this.Services().Settings.DefaultPhoneNumberDisplay);
                                this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], err);
							});
					}
					finally
					{
						_orderToCreate.IsPendingCreation = false;
						this.Services().Message.ShowProgress(false);
					}
				}).HandleErrors();
		}

		public delegate void OrderHandler(object sender,EventArgs args);

		public event OrderHandler OrderCompleted;
		public event OrderHandler NoMoreTaxiWaiting;
	}
}