using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using ServiceStack.ServiceClient.Web;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
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

		public CallboxOrderListViewModel() : base()
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
			get { return AppSettings.ApplicationName; }
		}

		public override void Load()
		{
			base.Load();
			_isClosed = false;

			_orderNotified = new List<int?>();

			_refreshTimer = Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(a => RefreshOrderStatus());                           

			_token = this.MessengerHub.Subscribe<OrderDeleted>(orderId =>
				{
					this.CancelOrder.Execute(orderId.Content);
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

				var orderStatus = AccountService.GetActiveOrdersStatus().ToList().OrderByDescending(o => o.IBSOrderId).Where(status => BookingService.IsCallboxStatusActive(status.IBSStatusId));
				InvokeOnMainThread(() =>
					{
						Orders.Clear();

						if ( ( _orderToCreate != null ) && ( _orderToCreate.Order != null ) && orderStatus.Any( os=>os.IBSOrderId == _orderToCreate.Order.IbsOrderId ) )
						{
							Console.WriteLine ( "Clearing pending order" );
							_orderToCreate = null;
						}
						else if (  ( _orderToCreate != null ) && ( _orderToCreate.Order != null ) && orderStatus.None( os=>os.IBSOrderId == _orderToCreate.Order.IbsOrderId ) )
						{
							Orders.Add(_orderToCreate.Order);
						}

						Orders.AddRange(orderStatus.Select(status => new CallboxOrderViewModel()
							{
								OrderStatus = status,
								CreatedDate = status.PickupDate,
								IbsOrderId = status.IBSOrderId,
								Id = status.OrderId
							}));

						if (  (!Orders.Any()) && ( _orderToCreate == null ) ) 
						{
							Console.WriteLine ( "Exiting pending order" );
							RequestNavigate<CallboxCallTaxiViewModel>(true);
							Close();
						}

						if(Orders.None(x => BookingService.IsCallboxStatusCompleted(x.OrderStatus.IBSStatusId)))
						{
							NoMoreTaxiWaiting(this, null);
						}
						else
						{
							foreach (var order in Orders)
							{
								if (BookingService.IsCallboxStatusCompleted(order.OrderStatus.IBSStatusId) && !_orderNotified.Any(c => c.Value.Equals(order.IbsOrderId)))
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
				return this.GetCommand(() => this.MessageService.ShowEditTextDialog(Resources.GetString("BookTaxiTitle"), 
					Resources.GetString("BookTaxiPassengerName"), 
					Resources.GetString("Ok"), 
					passengerName => CreateOrder(passengerName)));
			}
		}

		public IMvxCommand CancelOrder
		{
			get
			{
				return this.GetCommand<Guid>(orderId => 
					this.MessageService.ShowMessage(this.Resources.GetString("CancelOrderTitle"), 
						this.Resources.GetString("CancelOrderMessage"), 
						this.Resources.GetString("Yes"), () => 
						{
							MessageService.ShowProgress(true);

							try
							{
								BookingService.CancelOrder(orderId);
								RemoveOrderFromList(orderId);
							}
							catch (Exception ex)
							{
								Thread.Sleep( 500 );
								try
								{
									BookingService.CancelOrder(orderId);
									RemoveOrderFromList(orderId);
								}
								catch 
								{
									MessageService.ShowMessage(this.Resources.GetString("ServiceError_ErrorCreatingOrderMessage"), this.Resources.GetString("ErrorCancellingOrderTitle"));
								}
							}
							finally
							{
								MessageService.ShowProgress(false);
							}
						}, this.Resources.GetString("No"), () => { }));
			}
		}

		private void RemoveOrderFromList(Guid orderId)
		{
			var orderToRemove = Orders.FirstOrDefault(o => o.Id.Equals(orderId));

			if ( (_orderToCreate != null) && ( _orderToCreate.Order != null ) && ( orderToRemove.IbsOrderId == _orderToCreate.Order.IbsOrderId ))
			{
				_orderToCreate = null;             
			}
			InvokeOnMainThread ( ()=>
				{
					Orders.Remove(orderToRemove) ;
					if ( Orders.Count () ==  0 )
					{                                                   
						RequestNavigate<CallboxCallTaxiViewModel>(true);
						Close();
					}
				});
		}

		private void CreateOrder(string passengerName)
		{        
			MessageService.ShowProgress(true);

			if ( _orderToCreate == null )
			{
				_orderToCreate = new CreateOrderInfo { PassengerName = passengerName, IsPendingCreation = true }; 
			}

			Task.Factory.StartNew(() =>
				{
					try
					{
						var pickupAddress = AccountService.GetFavoriteAddresses().FirstOrDefault();

						var newOrderCreated = new CreateOrder();
						newOrderCreated.Id = Guid.NewGuid();
						newOrderCreated.Settings = AccountService.CurrentAccount.Settings;
						newOrderCreated.PickupAddress = pickupAddress;
						newOrderCreated.PickupDate = DateTime.Now;

						if (!string.IsNullOrEmpty(passengerName))
						{
							newOrderCreated.Note = string.Format(Resources.GetString("Callbox.passengerName"), passengerName);
							newOrderCreated.Settings.Name = passengerName;
						}
						else
						{
							newOrderCreated.Note = Resources.GetString("Callbox.noPassengerName");
							newOrderCreated.Settings.Name = Resources.GetString("NotSpecified");
						}      

						var orderInfo = BookingService.CreateOrder(newOrderCreated);

						InvokeOnMainThread (() =>
							{                    
								if (pickupAddress != null)
								{
									if (orderInfo.IBSOrderId.HasValue && orderInfo.IBSOrderId > 0)
									{
										orderInfo.Name = newOrderCreated.Settings.Name;
										var o = new CallboxOrderViewModel()
										{
											CreatedDate = DateTime.Now,
											IbsOrderId = orderInfo.IBSOrderId,
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
									MessageService.ShowMessage(Resources.GetString("ErrorCreatingOrderTitle"), Resources.GetString("NoPickupAddress"));
								}
							});
					}
					catch (Exception ex)
					{
						InvokeOnMainThread(() =>
							{
								var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
								string err = string.Format(Resources.GetString("ServiceError_ErrorCreatingOrderMessage"), settings.ApplicationName, Config.GetSetting( "DefaultPhoneNumberDisplay" ));
								MessageService.ShowMessage(Resources.GetString("ErrorCreatingOrderTitle"), err);
							});
					}
					finally
					{
						_orderToCreate.IsPendingCreation = false;
						MessageService.ShowProgress(false);
					}
				}).HandleErrors();
		}

		public delegate void OrderHandler(object sender,EventArgs args);

		public event OrderHandler OrderCompleted;
		public event OrderHandler NoMoreTaxiWaiting;
	}
}