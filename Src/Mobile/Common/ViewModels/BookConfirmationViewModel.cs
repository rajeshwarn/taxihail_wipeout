using System;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookConfirmationViewModel : BaseViewModel
	{
        public BookConfirmationViewModel(string order)
		{
	        Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);
            RideSettings = new RideSettingsViewModel(Order.Settings);
            RideSettings.OnPropertyChanged().Subscribe(p => FirePropertyChanged(() => RideSettings));
		}


		public override void Load()
		{
		    base.Load();
			ShowWarningIfNecessary();
			ShowFareEstimateAlertDialogIfNecessary();
		}

		public string FareEstimate
		{
			get
			{
				return FormatPrice(Order.Estimate.Price);
			}

		}

		public RideSettingsViewModel RideSettings { get; set; }

		public string VehicleName
		{
			get
			{

				return RideSettings != null ? RideSettings.VehicleTypeName : null;
			}
		}

		public string ChargeType
		{
			get
			{
				return RideSettings != null ? RideSettings.ChargeTypeName : null;
			}
		}

		public CreateOrder Order { get; private set; }

		public string OrderPassengerNumber
		{
			get { return Order.Settings.Passengers.ToString(CultureInfo.InvariantCulture); }
		}

		public string OrderLargeBagsNumber
		{
			get { return Order.Settings.LargeBags.ToString(CultureInfo.InvariantCulture); }
		}

		public string OrderName
		{
			get { return Order.Settings.Name; }
		}

		public string OrderPhone
		{
			get { return Order.Settings.Phone; }
		}

		public string OrderApt
		{
			get { return !string.IsNullOrEmpty(Order.PickupAddress.Apartment) ? Order.PickupAddress.Apartment : "N/A"; }
		}

		public string OrderRingCode
		{
			get { return !string.IsNullOrEmpty(Order.PickupAddress.RingCode) ? Order.PickupAddress.RingCode : "N/A"; }
		}

		public bool ShowPassengerName
		{
			get
			{
// ReSharper disable once RedundantAssignment
                var ret = true;
				try
				{
					ret = Boolean.Parse(ConfigurationManager.GetSetting("Client.ShowPassengerName"));
				}
				catch (Exception)
				{
					return false;
				}
				return ret;
			}
		}

		public bool ShowPassengerPhone
		{
			get
			{
// ReSharper disable once RedundantAssignment
				var ret = true;
				try
				{
					ret = Boolean.Parse(ConfigurationManager.GetSetting("Client.ShowPassengerPhone"));
				}
				catch (Exception)
				{
					return false;
				}
				return ret;
			}
		}

		public bool ShowPassengerNumber
		{
			get
			{
// ReSharper disable once RedundantAssignment
				var ret = true;
				try
				{
					ret = Boolean.Parse(ConfigurationManager.GetSetting("Client.ShowPassengerNumber"));
				}
				catch (Exception)
				{
					return false;
				}
				return ret;
			}
		}

		public IMvxCommand NavigateToRefineAddress
		{
			get
			{
				return GetCommand(() => RequestSubNavigate<RefineAddressViewModel, RefineAddressViewModel>(new Dictionary<string, string>
				{
					{ "apt", Order.PickupAddress.Apartment },
					{ "ringCode", Order.PickupAddress.RingCode },
					{ "buildingName", Order.PickupAddress.BuildingName },
				}, result =>
				{
					if (result == null)
						return;

					Order.PickupAddress.Apartment = result.AptNumber;
					Order.PickupAddress.RingCode = result.RingCode;
					Order.PickupAddress.BuildingName = result.BuildingName;
					InvokeOnMainThread(() =>
					{
						FirePropertyChanged(() => AptRingCode);
						FirePropertyChanged(() => BuildingName);
					});
				}));
			}
		}

		public IMvxCommand NavigateToEditInformations
		{
			get
			{
				return GetCommand(() => RequestSubNavigate<BookEditInformationViewModel, Order>(
					new {
                            order = Order.ToJson()
                        }.ToSimplePropertyDictionary(), result =>
				{
					if (result == null)
						return;

					Order.PickupAddress.Apartment = result.PickupAddress.Apartment;
					Order.PickupAddress.RingCode = result.PickupAddress.RingCode;
					Order.PickupAddress.BuildingName = result.PickupAddress.BuildingName;
					Order.Settings.Name = result.Settings.Name;
					Order.Settings.VehicleTypeId = result.Settings.VehicleTypeId;
					Order.Settings.ChargeTypeId = result.Settings.ChargeTypeId;
					Order.Settings.Phone = result.Settings.Phone;
					Order.Settings.Passengers = result.Settings.Passengers;
					Order.Settings.LargeBags = result.Settings.LargeBags;
					InvokeOnMainThread(() =>
					{
						FirePropertyChanged(() => RideSettings);
						FirePropertyChanged(() => AptRingCode);
						FirePropertyChanged(() => BuildingName);
						FirePropertyChanged(() => OrderPassengerNumber);
						FirePropertyChanged(() => OrderLargeBagsNumber);
						FirePropertyChanged(() => OrderPhone);
						FirePropertyChanged(() => OrderName);
						FirePropertyChanged(() => OrderApt);
						FirePropertyChanged(() => OrderRingCode);
						FirePropertyChanged(() => VehicleName);
						FirePropertyChanged(() => ChargeType);
					});
				}));
			}
		}

		public IMvxCommand ConfirmOrderCommand
		{
			get
			{

				return GetCommand(() =>
				{


					Order.Id = Guid.NewGuid();
					try
					{
						MessageService.ShowProgress(true);
						var orderInfo = BookingService.CreateOrder(Order);

							if (!orderInfo.IbsOrderId.HasValue || !(orderInfo.IbsOrderId > 0))
							return;

						var orderCreated = new Order
						{
							CreatedDate = DateTime.Now, 
							DropOffAddress = Order.DropOffAddress, 
							IbsOrderId = orderInfo.IbsOrderId, 
							Id = Order.Id, PickupAddress = Order.PickupAddress,
							Note = Order.Note, 
							PickupDate = Order.PickupDate.HasValue ? Order.PickupDate.Value : DateTime.Now,
							Settings = Order.Settings
						};
	    						
						RequestNavigate<BookingStatusViewModel>(new
						        {
						            order = orderCreated.ToJson(),
						            orderStatus = orderInfo.ToJson()
						        });	
						Close();
						MessengerHub.Publish(new OrderConfirmed(this, Order, false));
					}
					catch
					{
						InvokeOnMainThread(() =>
						{
							if (CallIsEnabled)
							{
								var err = string.Format(Resources.GetString("ServiceError_ErrorCreatingOrderMessage"), Settings.ApplicationName, Config.GetSetting("DefaultPhoneNumberDisplay"));
								MessageService.ShowMessage(Resources.GetString("ErrorCreatingOrderTitle"), err);
							}
							else
							{
								MessageService.ShowMessage(Resources.GetString("ErrorCreatingOrderTitle"), Resources.GetString("ServiceError_ErrorCreatingOrderMessage_NoCall"));
							}
						});
					}
					finally
					{
						MessageService.ShowProgress(false);
					}                         
				}); 
               
			}
		}

		public bool CallIsEnabled
		{
			get
			{

				return !Config.GetSetting("Client.HideCallDispatchButton", false);
			}

		}

		public IMvxCommand CancelOrderCommand
		{
			get
			{
				return GetCommand(() =>
				{
					Close();
					MessengerHub.Publish(new OrderConfirmed(this, Order, true));
				});            
			}
		}

		private async void ShowWarningIfNecessary()
		{
			var validationInfo = await BookingService.ValidateOrder(Order);
			if (validationInfo.HasWarning)
			{

				MessageService.ShowMessage(Resources.GetString("WarningTitle"), 
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
					validationInfo.Message, Resources.GetString("ContinueButton"), () => validationInfo.ToString(), Resources.GetString("CancelBoutton"), () => RequestClose(this));
			}
		}

        //todo refactorer ça, avec un getdefault value
		private bool ShowEstimate
		{
			get
			{
// ReSharper disable once RedundantAssignment
				var ret = true;
				try
				{
					ret = Boolean.Parse(ConfigurationManager.GetSetting("Client.ShowEstimate"));
				}
				catch (Exception)
				{
					return true;
				}
				return ret;
			}
		}

		private void ShowFareEstimateAlertDialogIfNecessary()
		{
			if (ShowEstimate)
			{
				var estimateEnabled = ConfigurationManager.GetSetting("Client.ShowEstimateWarning", true);

				if (estimateEnabled &&
				                CacheService.Get<string>("WarningEstimateDontShow").IsNullOrEmpty() &&
				                Order.DropOffAddress.HasValidCoordinate())
				{
					MessageService.ShowMessage(Resources.GetString("WarningEstimateTitle"), Resources.GetString("WarningEstimate"),
						"Ok", delegate
					{
					},
						Resources.GetString("WarningEstimateDontShow"), () => CacheService.Set("WarningEstimateDontShow", "yes"));
				}
			}
		}

		public string AptRingCode
		{
			get
			{
				return FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);
			}
		}

		public string BuildingName
		{
			get
			{
				return FormatBuildingName(Order.PickupAddress.BuildingName);
			}
		}

		private string FormatAptRingCode(string apt, string rCode)
		{
			string result = apt.HasValue() ? apt : Resources.GetString("ConfirmNoApt");
			result += @" / ";
			result += rCode.HasValue() ? rCode : Resources.GetString("ConfirmNoRingCode");
			return result;
		}

		private string FormatBuildingName(string buildingName)
		{
		    if (buildingName.HasValue())
			{
				return buildingName;
			}
		    return Resources.GetString(Resources.GetString("HistoryDetailBuildingNameNotSpecified"));
		}

	    private string FormatPrice(double? price)
	    {
	        if (price.HasValue)
			{
				var culture = ConfigurationManager.GetSetting("PriceFormat");
				return string.Format(new CultureInfo(culture), "{0:C}", price);
			}
	        return string.Empty;
	    }
	}
}

