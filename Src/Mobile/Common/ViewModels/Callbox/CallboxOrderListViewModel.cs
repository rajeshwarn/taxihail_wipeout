using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using ServiceStack.ServiceClient.Web;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Threading.Tasks;
using System.Threading;
using apcurium.MK.Booking.Mobile.Extensions;

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
			get { return this.Services().AppSettings.ApplicationName; }
		}

		public override void Load()
		{
			base.Load();
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

                var orderStatus = this.Services().Account.GetActiveOrdersStatus().ToList().OrderByDescending(o => o.IbsOrderId).Where(status => this.Services().Booking.IsCallboxStatusActive(status.IbsStatusId));
				InvokeOnMainThread(() =>
					{
						Orders.Clear();

					    var orderStatusDetails = orderStatus as OrderStatusDetail[] ?? orderStatus.ToArray();
					    if ( ( _orderToCreate != null ) && ( _orderToCreate.Order != null ) && orderStatusDetails.Any( os=>os.IbsOrderId == _orderToCreate.Order.IbsOrderId ) )
						{
							_orderToCreate = null;
						}
						else if (  ( _orderToCreate != null ) && ( _orderToCreate.Order != null ) && orderStatusDetails.None( os=>os.IbsOrderId == _orderToCreate.Order.IbsOrderId ) )
						{
							Orders.Add(_orderToCreate.Order);
						}

						Orders.AddRange(orderStatusDetails.Select(status => new CallboxOrderViewModel
							{
								OrderStatus = status,
								CreatedDate = status.PickupDate,
								IbsOrderId = status.IbsOrderId,
								Id = status.OrderId
							}));

						if (  (!Orders.Any()) && ( _orderToCreate == null ) ) 
						{
							RequestNavigate<CallboxCallTaxiViewModel>(true);
							Close();
						}

						if(Orders.None(x => this.Services().Booking.IsCallboxStatusCompleted(x.OrderStatus.IbsStatusId)))
						{
							NoMoreTaxiWaiting(this, null);
						}
						else
						{
							foreach (var order in Orders)
							{
								if (this.Services().Booking.IsCallboxStatusCompleted(order.OrderStatus.IbsStatusId) && !_orderNotified.Any(c => c != null && c.Value.Equals(order.IbsOrderId)))
								{
									_orderNotified.Add(order.IbsOrderId);
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

		protected override void Close()
		{
			base.Close();

			UnsubscribeToken();
		}

		public void UnsubscribeToken()
		{
			_refreshTimer.Dispose();
			_token.Dispose();
			_isClosed = true;
		}

		public IMvxCommand CallTaxi
		{
			get
			{
				return GetCommand(() => this.Services().Message.ShowEditTextDialog(this.Services().Resources.GetString("BookTaxiTitle"), 
					this.Services().Resources.GetString("BookTaxiPassengerName"), 
					this.Services().Resources.GetString("Ok"), 
					CreateOrder));
			}
		}

		public IMvxCommand CancelOrder
		{
			get
			{
				return GetCommand<Guid>(orderId => 
					this.Services().Message.ShowMessage(this.Services().Resources.GetString("CancelOrderTitle"), 
						this.Services().Resources.GetString("CancelOrderMessage"), 
						this.Services().Resources.GetString("Yes"), () => 
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
									this.Services().Message.ShowMessage(this.Services().Resources.GetString("ServiceError_ErrorCreatingOrderMessage"), this.Services().Resources.GetString("ErrorCancellingOrderTitle"));
								}
							}
							finally
							{
								this.Services().Message.ShowProgress(false);
							}
						}, this.Services().Resources.GetString("No"), () => { }));
			}
		}

		private void RemoveOrderFromList(Guid orderId)
		{
			var orderToRemove = Orders.FirstOrDefault(o => o.Id.Equals(orderId));

			if (orderToRemove != null && ((_orderToCreate != null) && ( _orderToCreate.Order != null ) && (orderToRemove.IbsOrderId == _orderToCreate.Order.IbsOrderId )))
			{
				_orderToCreate = null;             
			}
			InvokeOnMainThread ( ()=>
				{
					Orders.Remove(orderToRemove) ;
					if (!Orders.Any())
					{                                                   
						RequestNavigate<CallboxCallTaxiViewModel>(true);
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
							newOrderCreated.Note = string.Format(this.Services().Resources.GetString("Callbox.passengerName"), passengerName);
							newOrderCreated.Settings.Name = passengerName;
						}
						else
						{
							newOrderCreated.Note = this.Services().Resources.GetString("Callbox.noPassengerName");
							newOrderCreated.Settings.Name = this.Services().Resources.GetString("NotSpecified");
						}      

						var orderInfo = this.Services().Booking.CreateOrder(newOrderCreated);

						InvokeOnMainThread (() =>
							{                    
								if (pickupAddress != null)
								{
									if (orderInfo.IbsOrderId.HasValue && orderInfo.IbsOrderId > 0)
									{
										orderInfo.Name = newOrderCreated.Settings.Name;
										var o = new CallboxOrderViewModel
										{
											CreatedDate = DateTime.Now,
											IbsOrderId = orderInfo.IbsOrderId,
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
									this.Services().Message.ShowMessage(this.Services().Resources.GetString("ErrorCreatingOrderTitle"), this.Services().Resources.GetString("NoPickupAddress"));
								}
							});
					}
					catch
					{
						InvokeOnMainThread(() =>
							{
								string err = string.Format(this.Services().Resources.GetString("ServiceError_ErrorCreatingOrderMessage"), this.Services().Settings.ApplicationName, this.Services().Config.GetSetting( "DefaultPhoneNumberDisplay" ));
								this.Services().Message.ShowMessage(this.Services().Resources.GetString("ErrorCreatingOrderTitle"), err);
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