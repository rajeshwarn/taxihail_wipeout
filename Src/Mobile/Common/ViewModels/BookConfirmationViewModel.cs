using System;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Messages;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Globalization;
using apcurium.MK.Booking.Mobile.Navigation;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookConfirmationViewModel : BaseViewModel
	{
		public void Init(string order)
		{
			Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);
			RideSettings = new RideSettingsViewModel();
			RideSettings.Init(Order.Settings.ToJson());
			RideSettings.OnPropertyChanged().Subscribe(p => RaisePropertyChanged(() => RideSettings));
		}

		public override void OnViewLoaded()
		{
		    base.OnViewLoaded();
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
                    ret = Boolean.Parse(this.Services().Config.GetSetting("Client.ShowPassengerName"));
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
                    ret = Boolean.Parse(this.Services().Config.GetSetting("Client.ShowPassengerPhone"));
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
                    ret = Boolean.Parse(this.Services().Config.GetSetting("Client.ShowPassengerNumber"));
				}
				catch (Exception)
				{
					return false;
				}
				return ret;
			}
		}

        public AsyncCommand NavigateToEditInformations
		{
			get
			{
				return GetCommand(() => ShowSubViewModel<BookEditInformationViewModel, Order>(
					new
					{
                    	order = Order.ToJson()
					}, result =>
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

					RaisePropertyChanged(() => RideSettings);
					RaisePropertyChanged(() => AptRingCode);
					RaisePropertyChanged(() => BuildingName);
					RaisePropertyChanged(() => OrderPassengerNumber);
					RaisePropertyChanged(() => OrderLargeBagsNumber);
					RaisePropertyChanged(() => OrderPhone);
					RaisePropertyChanged(() => OrderName);
					RaisePropertyChanged(() => OrderApt);
					RaisePropertyChanged(() => OrderRingCode);
					RaisePropertyChanged(() => VehicleName);
					RaisePropertyChanged(() => ChargeType);
				}));
			}
		}

        public AsyncCommand ConfirmOrderCommand
		{
			get
			{
				return GetCommand(async () =>
				{
					Order.Id = Guid.NewGuid();
					using(this.Services().Message.ShowProgress())
					{
						try
						{
	                        this.Services().Message.ShowProgress(true);
								var orderInfo = await this.Services().Booking.CreateOrder(Order);

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
		    						
							ShowViewModel<BookingStatusViewModel>(new
							        {
							            order = orderCreated.ToJson(),
							            orderStatus = orderInfo.ToJson()
							        });	
							ChangePresentation(new ClearHistoryPresentationHint());
	                        this.Services().MessengerHub.Publish(new OrderConfirmed(this, Order, false));
						}
						catch
						{
							if (CallIsEnabled)
							{
	                            var err = string.Format(this.Services().Localize["ServiceError_ErrorCreatingOrderMessage"], this.Services().Settings.ApplicationName, this.Services().Config.GetSetting("DefaultPhoneNumberDisplay"));
	                            this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], err);
							}
							else
							{
	                            this.Services().Message.ShowMessage(this.Services().Localize["ErrorCreatingOrderTitle"], this.Services().Localize["ServiceError_ErrorCreatingOrderMessage_NoCall"]);
							}
						}
					}
					                         
				}); 
			}
		}

		public bool CallIsEnabled
		{
			get
			{
                return !this.Services().Config.GetSetting("Client.HideCallDispatchButton", false);
			}
		}

        public AsyncCommand CancelOrderCommand
		{
			get
			{
				return GetCommand(() =>
				{
					Close(this);
                    this.Services().MessengerHub.Publish(new OrderConfirmed(this, Order, true));
				});            
			}
		}

		private async void ShowWarningIfNecessary()
		{
            var validationInfo = await this.Services().Booking.ValidateOrder(Order);
			if (validationInfo.HasWarning)
			{
                this.Services().Message.ShowMessage(this.Services().Localize["WarningTitle"], 
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
					validationInfo.Message, this.Services().Localize["Continue"], () => validationInfo.ToString(), this.Services().Localize["Cancel"], () => Close(this));
			}
		}

        //todo refactorer a, avec un getdefault value
		private bool ShowEstimate
		{
			get
			{
// ReSharper disable once RedundantAssignment
				var ret = true;
				try
				{
                    ret = Boolean.Parse(this.Services().Config.GetSetting("Client.ShowEstimate"));
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
                var estimateEnabled = this.Services().Config.GetSetting("Client.ShowEstimateWarning", true);

				if (estimateEnabled &&
                                this.Services().Cache.Get<string>("WarningEstimateDontShow").IsNullOrEmpty() &&
				                Order.DropOffAddress.HasValidCoordinate())
				{
                    this.Services().Message.ShowMessage(this.Services().Localize["WarningEstimateTitle"], this.Services().Localize["WarningEstimate"],
						"Ok", delegate
					{
					},
                        this.Services().Localize["WarningEstimateDontShow"], () => this.Services().Cache.Set("WarningEstimateDontShow", "yes"));
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
			string result = apt.HasValue() ? apt : this.Services().Localize["NoAptText"];
			result += @" / ";
			result += rCode.HasValue() ? rCode : this.Services().Localize["NoRingCodeText"];
			return result;
		}

		private string FormatBuildingName(string buildingName)
		{
		    if (buildingName.HasValue())
			{
				return buildingName;
			}
			return this.Services().Localize["BuildingNameText"];
		}

	    private string FormatPrice(double? price)
	    {
	        if (price.HasValue)
			{
                var culture = this.Services().Config.GetSetting("PriceFormat");
				return string.Format(new CultureInfo(culture), "{0:C}", price);
			}
	        return string.Empty;
	    }
	}
}

