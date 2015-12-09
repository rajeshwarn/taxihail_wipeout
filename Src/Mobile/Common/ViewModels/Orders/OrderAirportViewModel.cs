using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Helpers;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderAirportViewModel : BaseViewModel
	{
        private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAirportInformationService _airportInformationService;

		private const string NoAirlines = "No Airline";
        private const string PUCurbSide = "Curb Side";

		private List<ListItem> _pickupPoints;
		private int _selectedPickupPointsId;
		private List<ListItem> _airlines;
		private int? _airLineId;
		private BookingSettings _bookingSettings;
		private string _flightNumber;
		private string _pickupTimeStamp;
		private string _note;
		private Address _pickupAddress;
		private PickupPoint[] _poiPickup;
		private KeyValuePair<int, Airline>[] _carrierCodes = new KeyValuePair<int, Airline>[0];
		private Airline[] _poiAirline = new Airline[0];
		private DateTime? _pickupDate;

		public OrderAirportViewModel(IOrderWorkflowService orderWorkflowService, IAirportInformationService airportInformationService)
		{
			_orderWorkflowService = orderWorkflowService;
	        _airportInformationService = airportInformationService;

	        Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
            Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
            Observe(_orderWorkflowService.GetAndObservePOIRefPickupList(), poiPickup => POIPickup = poiPickup);
            Observe(_orderWorkflowService.GetAndObservePOIRefAirlineList(), poiAirline => POIAirline = poiAirline);
			Observe(_orderWorkflowService.GetAndObservePickupDate(), pickupDate => PickupDate = pickupDate);

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

		#region Properties
		public List<KeyValuePair<int, PickupPoint>> PickupPointItems { get; set; }

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

		public int SelectedPickupPointsId
		{
			get
			{
				return _selectedPickupPointsId;
			}
			set
			{
				if (_selectedPickupPointsId == value)
				{
					return;
				}

				_selectedPickupPointsId = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => SelectedPickupPointName);
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

		public int? AirlineId
		{
			get
			{
				return _airLineId;
			}
			set
			{
				if (_airLineId == value)
				{
					return;
				}

				_airLineId = value;
				_airLineId = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => AirlineName);
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

				return vehicle == null
					? string.Empty
					: vehicle.Display;
			}
		}

		public BookingSettings BookingSettings
		{
			get { return _bookingSettings; }
			set
			{
				if (_bookingSettings == value)
				{
					return;
				}

				_bookingSettings = value;
				RaisePropertyChanged();
			}
		}

		public string FlightNumber
		{
			get { return _flightNumber; }
			set
			{
				if (_flightNumber == value)
				{
					return;
				}

				_flightNumber = value;
				RaisePropertyChanged();
			}
		}

		public string Title
		{
			get { return _pickupAddress.FullAddress; }
		}

		public string PickupTimeStamp
		{
			get { return _pickupTimeStamp; }
			set
			{
				if (_pickupTimeStamp == value)
				{
					return;
				}

				_pickupTimeStamp = value;
				RaisePropertyChanged();
			}
		}

		public string Note
		{
			get { return _note; }
			set
			{
				if (_note == value)
				{
					return;
				}

				_note = value;
				_orderWorkflowService.SetNoteToDriver(value);
				RaisePropertyChanged();
			}
		}

		public Address PickupAddress
		{
			get { return _pickupAddress; }
			set
			{
				if (_pickupAddress == value)
				{
					return;
				}

				_pickupAddress = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => Title);

				if (_pickupAddress != null && _pickupAddress.AddressLocationType == AddressLocationType.Airport)
				{
					_orderWorkflowService.POIRefAirLineList(string.Empty, 0);
					_orderWorkflowService.POIRefPickupList(string.Empty, 0);

					// Clear/default any previous data
					AirlineId = -1;
					RaisePropertyChanged(() => AirlineName);
					FlightNumber = string.Empty;
					SelectedPickupPointsId = 0;
					RaisePropertyChanged(() => SelectedPickupPointName);
				}
			}
		}

		public PickupPoint[] POIPickup
		{
			get { return _poiPickup; }
			set
			{
				if (_poiPickup == value || value == null)
				{
					return;
				}

				_poiPickup = value;
				if (_poiPickup.None())
				{
					return;
				}

				PickupPointItems.Clear();

				var pickupPoints = _poiPickup
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

		public DateTime? PickupDate
		{
			get { return _pickupDate; }
			set
			{
				_pickupDate = value;

				PickupTimeStamp = _pickupDate.HasValue
					? _pickupDate.Value.ToString("g")
					: this.Services().Localize["TimeNow"];

				RaisePropertyChanged();
			}
		}

		#endregion

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
			try
			{		
				var carrier = _carrierCodes.FirstOrDefault(c => c.Key == AirlineId);

				return await _airportInformationService.GetTerminal(PickupDate ?? DateTime.Now, FlightNumber, carrier.Value.Id, AirlineName, _pickupAddress.PlaceId, true);
			}
			catch (Exception ex)
			{
				Logger.LogMessage("An error has occurred while attempting to get the airport terminal.");
				Logger.LogError(ex);

				return string.Empty;
			}
		}

		private bool CanGetTerminal()
		{
			return AirlineId.HasValue
			    && FlightNumber.HasValue() 
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
			if (pickupPointValue != null && pickupPointValue.AdditionalFee.HasValue())
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

				if (!terminal.HasValue())
				{
					return;
				}

				UpdateDriverNoteWithAirportInformation(pickupPointValue, fee, terminal);

				((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Review;

				// Clear all values...
				AirlineId = -1;
				FlightNumber = string.Empty;
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
				sb.AppendLine(Note);
			}

			sb.Append(localize["BookingAirportDetails"], _pickupAddress.FullAddress, AirlineName, FlightNumber, SelectedPickupPointName);

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