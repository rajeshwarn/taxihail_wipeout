using System;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
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

        public AsyncCommand NavigateToRefineAddress
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

        public AsyncCommand NavigateToEditInformations
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

        public AsyncCommand ConfirmOrderCommand
		{
			get
			{

				return GetCommand(() =>
				{


					Order.Id = Guid.NewGuid();
					try
					{
                        this.Services().Message.ShowProgress(true);
                        var orderInfo = this.Services().Booking.CreateOrder(Order);

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
                        this.Services().MessengerHub.Publish(new OrderConfirmed(this, Order, false));
					}
					catch
					{
						InvokeOnMainThread(() =>
						{
							if (CallIsEnabled)
							{
                                var err = string.Format(this.Services().Resources.GetString("ServiceError_ErrorCreatingOrderMessage"), this.Services().Settings.ApplicationName, this.Services().Config.GetSetting("DefaultPhoneNumberDisplay"));
                                this.Services().Message.ShowMessage(this.Services().Resources.GetString("ErrorCreatingOrderTitle"), err);
							}
							else
							{
                                this.Services().Message.ShowMessage(this.Services().Resources.GetString("ErrorCreatingOrderTitle"), this.Services().Resources.GetString("ServiceError_ErrorCreatingOrderMessage_NoCall"));
							}
						});
					}
					finally
					{
                        this.Services().Message.ShowProgress(false);
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
					Close();
                    this.Services().MessengerHub.Publish(new OrderConfirmed(this, Order, true));
				});            
			}
		}

		private async void ShowWarningIfNecessary()
		{
            var validationInfo = await this.Services().Booking.ValidateOrder(Order);
			if (validationInfo.HasWarning)
			{

                this.Services().Message.ShowMessage(this.Services().Resources.GetString("WarningTitle"), 
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    validationInfo.Message, this.Services().Resources.GetString("ContinueButton"), () => validationInfo.ToString(), this.Services().Resources.GetString("CancelBoutton"), () => RequestClose(this));
			}
		}

        //todo refactorer �a, avec un getdefault value
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
                    this.Services().Message.ShowMessage(this.Services().Resources.GetString("WarningEstimateTitle"), this.Services().Resources.GetString("WarningEstimate"),
						"Ok", delegate
					{
					},
                        this.Services().Resources.GetString("WarningEstimateDontShow"), () => this.Services().Cache.Set("WarningEstimateDontShow", "yes"));
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
            string result = apt.HasValue() ? apt : this.Services().Resources.GetString("ConfirmNoApt");
			result += @" / ";
            result += rCode.HasValue() ? rCode : this.Services().Resources.GetString("ConfirmNoRingCode");
			return result;
		}

		private string FormatBuildingName(string buildingName)
		{
		    if (buildingName.HasValue())
			{
				return buildingName;
			}
            return this.Services().Resources.GetString(this.Services().Resources.GetString("HistoryDetailBuildingNameNotSpecified"));
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

