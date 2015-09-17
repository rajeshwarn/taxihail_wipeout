using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Text;
using MK.Common.Android.Helpers;
using Newtonsoft.Json.Linq;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderAirportViewModel : BaseViewModel
	{
        private readonly IOrderWorkflowService _orderWorkflowService;
		private IAirportInformationService _airportInformationService;

		private const string NoAirlines = "No Airline";
        private const string PUCurbSide = "Curb Side";


        public class PointsItems
        {
            public string Name = string.Empty;
            public string Fee = string.Empty;
        }

        public List<PointsItems> PickupPoints { get; set; }

        public OrderAirportViewModel(IOrderWorkflowService orderWorkflowService, IAirportInformationService airportInformationService)
		{
			_orderWorkflowService = orderWorkflowService;
	        _airportInformationService = airportInformationService;

	        Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
            Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
            Observe(_orderWorkflowService.GetAndObservePOIRefPickupList(), pObject => POIPickup = pObject);
            Observe(_orderWorkflowService.GetAndObservePOIRefAirlineList(), poiAirline => POIAirline = poiAirline);
            Observe(_orderWorkflowService.GetAndObservePickupDate(), DateUpdated);

            //We are throttling to prevent cases where we can cause the app to become unresponsive after typing fast.
            Observe(_orderWorkflowService.GetAndObserveNoteToDriver().Throttle(TimeSpan.FromMilliseconds(500)), note => Note = note);

			Observe(GetTerminals(), terminal => Terminal = terminal);
        }

		public string Terminal
		{
			get { return _terminal; }
			set
			{
				_terminal = value;
				RaisePropertyChanged();
			}
		}

		public void Init()
        {
            Airlines = new List<ListItem>
            {
				new ListItem {Display = NoAirlines, Id = 0},
            };
            AirlineId = 0;
            RaisePropertyChanged(() => AirlineName);

	        PickupPoints = new List<PointsItems>
	        {
		        new PointsItems {Name = PUCurbSide, Fee = string.Empty}
	        };

	        _pUPoints = new List<ListItem>();
            for( var iIndex = 0; iIndex < PickupPoints.Count; iIndex++ )
            {
                var pItem = PickupPoints[iIndex];
                _pUPoints.Add( new ListItem { Display = pItem.Name, Id = iIndex });

            }
            PUPointsId = 0;
            RaisePropertyChanged(() => PUPointsName);
        }

        private List<ListItem> _pUPoints;
        public List<ListItem> PUPoints
        {
            get
            {
                return _pUPoints;
            }
            set
            {
                _airlines = value ?? new List<ListItem>();
                RaisePropertyChanged();
                RaisePropertyChanged(() => PUPointsId);
                RaisePropertyChanged(() => PUPointsName);

            }
        }

        private int _pUPointsId;
        public int PUPointsId
        {
            get
            {
                return _pUPointsId;
            }
            set
            {
                if (value != _pUPointsId)
                {
                    _pUPointsId = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(() => PUPointsName);
                }
            }
        }

        public string PUPointsName
        {
            get
            {
                if (PUPoints == null)
                {
                    return null;
                }

                var vehicle = PUPoints.FirstOrDefault(x => x.Id == PUPointsId) ?? PUPoints.FirstOrDefault();

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

                var vehicle = Airlines.FirstOrDefault(x => x.Id == AirlineId);
                if (vehicle == null)
                {
                    vehicle = Airlines.FirstOrDefault();
                }

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

		public DateTime? PickupDateTime
		{
			get { return _pickupDateTime; }
			set
			{
				_pickupDateTime = value; 
				DateUpdated(value);

				RaisePropertyChanged();
			}
		}


		private void DateUpdated(DateTime? date)
        {
            PickupTimeStamp = date.HasValue
                ? date.Value.ToShortDateString() + " " + date.Value.ToShortTimeString()
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
		            AirlineId = 0;
		            RaisePropertyChanged(() => AirlineName);
		            FlightNum = string.Empty;
		            PUPointsId = 0;
		            RaisePropertyChanged(() => PUPointsName);
	            }
            }
        }

        private string _poiPickup;
        public string POIPickup
        {
            get { return _poiPickup; }
            set
            {
	            if (value == _poiPickup || value == null)
	            {
		            return;
	            }

	            _poiPickup = value;
	            if (value == string.Empty)
	            {
		            return;
	            }

	            var pArray = JArray.Parse(value);

	            PickupPoints.Clear();
	            foreach (var pItem in pArray)
	            {
		            var sFee = string.Empty;
		            var sName = string.Empty;
		            foreach (var x in pItem)
		            {
			            if (((JProperty)x).Name == "additionalFee")
			            {
				            sFee = ((string)((JProperty)x).Value);
			            }
			            else if (((JProperty)x).Name == "name")
			            {
				            sName = ((string)((JProperty)x).Value);
			            }
		            }
		            PickupPoints.Add(new PointsItems { Name = sName, Fee = sFee });
	            }
	            _pUPoints = new List<ListItem>();
	            for (var iIndex = 0; iIndex < PickupPoints.Count; iIndex++)
	            {
		            var pItem = PickupPoints[iIndex];
		            _pUPoints.Add(new ListItem { Display = pItem.Name, Id = iIndex });

	            }
	            PUPointsId = 0;
	            RaisePropertyChanged(() => PUPoints);
	            RaisePropertyChanged(() => PUPointsName);
            }
        }

		private KeyValuePair<int, Airline>[] _carrierCodes = new KeyValuePair<int, Airline>[0];

		private string _terminal;
		private Airline[] _poiAirline = new Airline[0];
		private DateTime? _pickupDateTime;

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

	            AirlineId = 0;
	            RaisePropertyChanged(() => Airlines);
	            RaisePropertyChanged(() => AirlineName);
            }
        }

		private void UpdateAirlines(IEnumerable<Airline> airlines)
		{
			_carrierCodes = airlines
				.Select((airline, index) => new KeyValuePair<int, Airline>(index, airline))
				.ToArray();

			_airlines.AddRange(_carrierCodes.Select(carrier => new ListItem { Display = carrier.Value.Name, Id = carrier.Key }));
		}

		public IObservable<string> GetTerminals()
		{
			return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
					h => PropertyChanged += h,
					h => PropertyChanged -= h
				)
				.Where(args => args.EventArgs.PropertyName.Equals("AirlineId")
					|| args.EventArgs.PropertyName.Equals("PickupDateTime")
				    || args.EventArgs.PropertyName.Equals("FlightNum")
				)
				.Where(_ => CanGetTerminal())
				.SelectMany(date =>
				{
					var carrier = _carrierCodes.FirstOrDefault(c => c.Key == AirlineId);

					var carrierCode = carrier.Value.Id.Replace("utog.", "");

					return _airportInformationService.GetTerminal(PickupDateTime.Value, FlightNum, carrierCode, _pickupAddress.PlaceId);
				});
		}

		private bool CanGetTerminal()
		{
			return AirlineId.HasValue
				   && PickupDateTime.HasValue
			       && FlightNum.HasValue() 
			       && _pickupAddress.SelectOrDefault(addr => addr.PlaceId.HasValue())
			       && _carrierCodes.Any(c => c.Key == AirlineId);
		}

		public ICommand NextCommand
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    // check if additional fee is accepted
                    var pItem = PickupPoints[PUPointsId];
                    var accepted = true;
                    var fee = 0;
                    if (pItem != null && pItem.Fee != string.Empty)
                    {
	                    foreach (var c in pItem.Fee.Where(char.IsDigit))
	                    {
		                    fee *= 10;
		                    // When converting, subtract 0x30 to get true value between 0 - 9
		                    fee += Convert.ToInt32(c - 0x30);
	                    }

	                    if( fee > 0 )
						{
	                        accepted = false;
	                        var feeWarning = string.Format(this.Services().Localize["BookingAirportPickupPointFee"], pItem.Name, pItem.Fee);
	                        await this.Services().Message.ShowMessage(
	                            this.Services().Localize["WarningTitle"],
	                            feeWarning,
	                            this.Services().Localize["OkButtonText"],
	                            () =>
	                            {
	                                accepted = true;
	                            },
	                            this.Services().Localize["Cancel"],
	                            () => { });
						}
                    }
	                if (accepted)
                    {
                        var sb = new StringBuilder();
                        if( Note.Length > 0 )
                        {
                            sb.Append("{0}\n", Note );
                        }
						sb.Append(this.Services().Localize["BookingAirportDetails"], _pickupAddress.FullAddress, AirlineName, FlightNum, PUPointsName);
                        if (pItem != null && pItem.Fee != string.Empty && fee > 0 )
                        {
                            sb.Append(this.Services().Localize["BookingAirportDetailsFee"], pItem.Fee);
                        }
                        Note = sb.ToString();

						((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Review;

                        // Clear all values...
                        AirlineId = 0;
                        FlightNum = string.Empty;
                    }
                    else
                    {
                        PUPointsId = 0;
                        RaisePropertyChanged();
                        RaisePropertyChanged(() => PUPointsName);
                    }
                });
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