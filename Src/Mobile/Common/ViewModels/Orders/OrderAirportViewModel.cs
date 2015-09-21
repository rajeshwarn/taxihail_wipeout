using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Text;
using System.Threading.Tasks;
using MK.Common.Android.Helpers;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderAirportViewModel : BaseViewModel
	{
        private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAirportInformationService _airportInformationService;

		private const string NoAirlines = "No Airline";
        private const string PUCurbSide = "Curb Side";

        public List<KeyValuePair<int, PickupPoint>> PickupPointItems { get; set; }

        public OrderAirportViewModel(IOrderWorkflowService orderWorkflowService, IAirportInformationService airportInformationService)
		{
			_orderWorkflowService = orderWorkflowService;
	        _airportInformationService = airportInformationService;

	        Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
            Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
            Observe(_orderWorkflowService.GetAndObservePOIRefPickupList(), poiPickup => POIPickup = poiPickup);
            Observe(_orderWorkflowService.GetAndObservePOIRefAirlineList(), poiAirline => POIAirline = poiAirline);
            Observe(_orderWorkflowService.GetAndObservePickupDate(), DateUpdated);

            //We are throttling to prevent cases where we can cause the app to become unresponsive after typing fast.
            Observe(_orderWorkflowService.GetAndObserveNoteToDriver().Throttle(TimeSpan.FromMilliseconds(500)), note => Note = note);
        }

		public void Init()
        {
            Airlines = new List<ListItem>
            {
				new ListItem {Display = NoAirlines, Id = -1},
            };
            AirlineId = -1;
            RaisePropertyChanged(() => AirlineName);


			var pickupPoint = new PickupPoint()
			{
				Name = PUCurbSide,
				AdditionalFee = string.Empty
			};

			PickupPointItems = new List<KeyValuePair<int, PickupPoint>>
	        {
		        new KeyValuePair<int, PickupPoint>(0, pickupPoint) 
	        };

			_pickupPoints = PickupPointItems
				.Select(item => new ListItem
				{
					Display = item.Value.Name,
					Id = item.Key
				})
				.ToList();

            SelectedPickupPointsId = 0;
            RaisePropertyChanged(() => SelectedPickupPointName);
        }

        private List<ListItem> _pickupPoints;
        public List<ListItem> PickupPoints
        {
            get
            {
                return _pickupPoints;
            }
            set
            {
                _airlines = value ?? new List<ListItem>();
                RaisePropertyChanged();
                RaisePropertyChanged(() => SelectedPickupPointsId);
                RaisePropertyChanged(() => SelectedPickupPointName);

            }
        }

        private int _selectedPickupPointsId;
        public int SelectedPickupPointsId
        {
            get
            {
                return _selectedPickupPointsId;
            }
            set
            {
                if (value != _selectedPickupPointsId)
                {
                    _selectedPickupPointsId = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(() => SelectedPickupPointName);
                }
            }
        }

        public string SelectedPickupPointName
        {
            get
            {
                if (PickupPoints == null)
                {
                    return null;
                }

                var vehicle = PickupPoints.FirstOrDefault(x => x.Id == SelectedPickupPointsId) ?? PickupPoints.FirstOrDefault();

	            return vehicle == null
					? string.Empty
					: vehicle.Display;
            }
        }

        private List<ListItem> _airlines;
        public List<ListItem> Airlines
        {
            get
            {
                return _airlines;
            }
            set
            {
                _airlines = value ?? new List<ListItem>();
                RaisePropertyChanged();
                RaisePropertyChanged(() => AirlineId);
                RaisePropertyChanged(() => AirlineName);

            }
        }

        private int? _airLineId;
        public int? AirlineId
        {
            get
            {
                return _airLineId;
            }
            set
            {
                if (value != _airLineId)
                {
                    _airLineId = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(() => AirlineName);
                }
            }
        }

        public string AirlineName
        {
            get
            {
                if (!AirlineId.HasValue)
                {
                    return this.Services().Localize["NoPreference"];
                }

                if (Airlines == null)
                {
                    return null;
                }

                var vehicle = Airlines.FirstOrDefault(x => x.Id == AirlineId) ?? Airlines.FirstOrDefault();

	            return vehicle==null 
					? string.Empty 
					: vehicle.Display;
            }
        }

        private BookingSettings _bookingSettings;
        public BookingSettings BookingSettings
        {
            get { return _bookingSettings; }
            set
            {
                if (value != _bookingSettings)
                {
                    _bookingSettings = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private string _flightNum;
		public string FlightNum
        {
            get { return _flightNum; }
            set
            {
                if (value != _flightNum)
                {
                    _flightNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Title
        {
            get { return _pickupAddress.FullAddress; }
        }
		

		private void DateUpdated(DateTime? date)
        {
            PickupTimeStamp = date.HasValue
				? date.Value.ToString("g")
                : this.Services().Localize["TimeNow"];
        }

        private string _pickupTimeStamp;
        public string PickupTimeStamp
        {
            get { return _pickupTimeStamp; }
            set
            {
                if (value != _pickupTimeStamp)
                {
                    _pickupTimeStamp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _note;
        public string Note
        {
            get { return _note; }
            set
            {
                if (_note != value)
                {
                    _note = value;
                    _orderWorkflowService.SetNoteToDriver(value);
                    RaisePropertyChanged();
                }
            }
        }

        private Address _pickupAddress;
        public Address PickupAddress
        {
            get { return _pickupAddress; }
            set
            {
	            if (value == _pickupAddress)
	            {
		            return;
	            }

	            _pickupAddress = value;
	            RaisePropertyChanged();
	            RaisePropertyChanged(() => Title);
	            if ((_pickupAddress != null) && (_pickupAddress.AddressLocationType == AddressLocationType.Airport))
	            {
		            _orderWorkflowService.POIRefAirLineList(string.Empty, 0);
		            _orderWorkflowService.POIRefPickupList(string.Empty, 0);

		            // Clear/default any previous data
		            AirlineId = -1;
		            RaisePropertyChanged(() => AirlineName);
		            FlightNum = string.Empty;
		            SelectedPickupPointsId = 0;
		            RaisePropertyChanged(() => SelectedPickupPointName);
	            }
            }
        }

		private PickupPoint[] _poiPickup;
		public PickupPoint[] POIPickup
        {
            get { return _poiPickup; }
            set
            {
	            if (value == _poiPickup || value == null)
	            {
		            return;
	            }

	            _poiPickup = value;
	            if (value.None())
	            {
		            return;
	            }
				
				PickupPointItems.Clear();

	            var pickupPoints = value
					.Select((point, index) => new KeyValuePair<int, PickupPoint>(index, point))
					.ToArray();

				PickupPointItems.AddRange(pickupPoints);

	            _pickupPoints = pickupPoints
		            .Select(item => new ListItem
		            {
			            Display = item.Value.Name,
			            Id = item.Key
		            })
		            .ToList();

	            SelectedPickupPointsId = 0;
	            RaisePropertyChanged(() => PickupPoints);
	            RaisePropertyChanged(() => SelectedPickupPointName);
            }
        }

		private KeyValuePair<int, Airline>[] _carrierCodes = new KeyValuePair<int, Airline>[0];
		private Airline[] _poiAirline = new Airline[0];

		public Airline[] POIAirline
        {
            get { return _poiAirline; }
            set
            {
				if (value == null || value.SequenceEqual(_poiAirline, new AirlineComparer()))
	            {
		            return;
	            }

	            _poiAirline = value;
	            if (value.None())
	            {
		            return;
	            }

	            UpdateAirlines(value);

	            AirlineId = -1;
	            RaisePropertyChanged(() => Airlines);
	            RaisePropertyChanged(() => AirlineName);
            }
        }

		private void UpdateAirlines(IEnumerable<Airline> airlines)
		{
			_carrierCodes = airlines
				.OrderBy(item => item.Name)
				.Select((airline, index) => new KeyValuePair<int, Airline>(index, airline))
				.ToArray();

			_airlines.AddRange(_carrierCodes.Select(carrier => new ListItem { Display = carrier.Value.Name, Id = carrier.Key }));
		}

		private async Task<string> GetTerminal()
		{
			var localize = this.Services().Localize;

			try
			{
				var date = await _orderWorkflowService
						.GetAndObservePickupDate()
						.Take(1)
						.ToTask();
				
				var carrier = _carrierCodes.FirstOrDefault(c => c.Key == AirlineId);

				var carrierCode = carrier.Value.Id.Replace("utog.", "").ToLowerInvariant();

				return await _airportInformationService.GetTerminal(date ?? DateTime.Now, FlightNum, carrierCode, _pickupAddress.PlaceId, true);
			}
			catch (WebServiceException ex)
			{
				var showMessage = false;

				if (ex.StatusCode == (int) HttpStatusCode.NoContent)
				{
					return "N/A";
				}

				if (ex.StatusCode == (int) HttpStatusCode.BadRequest)
				{
					this.Services().Message
						.ShowMessage(localize["Error"], string.Format(localize[ex.ErrorCode], AirlineName, FlightNum, PickupTimeStamp))
						.FireAndForget();

					return string.Empty;
				}

				if (ex.StatusCode == (int) HttpStatusCode.NotFound)
				{
					showMessage = true;
				}

				Logger.LogMessage("An error has occurred while attempting to get the airport terminal.");
				Logger.LogError(ex);

				if (!showMessage)
				{
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage("An error has occurred while attempting to get the airport terminal.");
				Logger.LogError(ex);

				return string.Empty;
			}

			var cancel = false;

			await this.Services().Message.ShowMessage(
						localize["BookingAirportNoFlights_Title"],
						string.Format(localize["BookingAirportNoFlights_Message"], AirlineName, FlightNum, PickupTimeStamp),
						localize["YesButton"],
						() => { },
						localize["NoButton"],
						() => cancel = true);


			return cancel 
				? string.Empty 
				: "N/A";
		}

		private bool CanGetTerminal()
		{
			return AirlineId.HasValue
			    && FlightNum.HasValue() 
			    && _pickupAddress.SelectOrDefault(addr => addr.PlaceId.HasValue())
			    && _carrierCodes.Any(c => c.Key == AirlineId);
		}

		private async Task ExecuteNext()
		{
			var localize = this.Services().Localize;

			// check if additional fee is accepted
			var pickupPoint = PickupPointItems
				.Select(item => (KeyValuePair<int, PickupPoint>?)item)
				.FirstOrDefault(item => item.HasValue && item.Value.Key == SelectedPickupPointsId);

			var pickupPointValue = pickupPoint.HasValue
				? pickupPoint.Value.Value
				: null;

			var accepted = true;
			var fee = 0;
			if (pickupPointValue != null && pickupPointValue.AdditionalFee != string.Empty)
			{
				foreach (var c in pickupPointValue.AdditionalFee.Where(char.IsDigit))
				{
					fee *= 10;
					// When converting, subtract 0x30 to get true value between 0 - 9
					fee += Convert.ToInt32(c - 0x30);
				}

				if (fee > 0)
				{
					accepted = false;
					var feeWarning = string.Format(localize["BookingAirportPickupPointFee"], pickupPointValue.Name, pickupPointValue.AdditionalFee);
					await this.Services().Message.ShowMessage(
						localize["WarningTitle"],
						feeWarning,
						localize["OkButtonText"],
						() => accepted = true,
						localize["Cancel"],
						() => { });
				}
			}

			if (!CanGetTerminal())
			{
				await this.Services().Message.ShowMessage(localize["BookingAirportMissingInfo_Title"], localize["BookingAirportMissingInfo_Message"]);

				return;
			}

			if (accepted)
			{
				var terminal = await GetTerminal().ShowProgress();

				if (terminal.HasValue())
				{
					return;
				}

				UpdateDriverNoteWithAirportInformation(pickupPointValue, fee, terminal);

				((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Review;

				// Clear all values...
				AirlineId = -1;
				FlightNum = string.Empty;
			}
			else
			{
				SelectedPickupPointsId = 0;
				RaisePropertyChanged();
				RaisePropertyChanged(() => SelectedPickupPointName);
			}
		}

		private void UpdateDriverNoteWithAirportInformation(PickupPoint pickupPoint, int fee, string terminal)
		{
			var localize = this.Services().Localize;

			var sb = new StringBuilder();

			if (Note.Length > 0)
			{
				sb.Append("{0}\n", Note);
			}

			sb.Append(localize["BookingAirportDetails"], _pickupAddress.FullAddress, AirlineName, FlightNum, SelectedPickupPointName);

			if (pickupPoint != null && pickupPoint.AdditionalFee != string.Empty && fee > 0)
			{
				sb.Append(localize["BookingAirportDetailsFee"], pickupPoint.AdditionalFee);
			}

			if (terminal.HasValue())
			{
				sb.Append(localize["BookingAirportDetailsTerminal"], terminal);
			}

			Note = sb.ToString();
		}

		public ICommand NextCommand
        {
            get
            {
                return this.GetCommand(() => ExecuteNext());
            }
        }

		public ICommand NavigateToDatePicker
        {
            get
            {
				return this.GetCommand(() => ((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.AirportPickDate);
            }
		}
	}
}