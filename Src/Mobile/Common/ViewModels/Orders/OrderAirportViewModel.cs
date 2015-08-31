using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Helpers;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration.Impl;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderAirportViewModel : BaseViewModel
	{
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IAccountService _accountService;
        private readonly IPaymentService _paymentService;

        private const string NoAirlines = "No Airline";
        private const string PUCurbSide = "Curb Side";


        public class PointsItems
        {
            public string Name = string.Empty;
            public string Fee = string.Empty;
        }

        public List<PointsItems> PickupPoints { get; set; }

        public OrderAirportViewModel(IOrderWorkflowService orderWorkflowService, IAccountService accountService, IPaymentService paymentService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
            _paymentService = paymentService;

            Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
            Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
            Observe(_orderWorkflowService.GetAndObservePOIRefPickupList(), pObject => POIPickup = pObject);
            Observe(_orderWorkflowService.GetAndObservePOIRefAirlineList(), pObject => POIAirline = pObject);
            Observe(_orderWorkflowService.GetAndObservePickupDate(), DateUpdated);

            //We are throttling to prevent cases where we can cause the app to become unresponsive after typing fast.
            Observe(_orderWorkflowService.GetAndObserveNoteToDriver().Throttle(TimeSpan.FromMilliseconds(500)), note => Note = note);

        }

        public async Task Init()
        {
            _airlines = new List<ListItem>
            {
				new ListItem {Display = NoAirlines, Id = 0},
            };
            AirlineId = 0;
            RaisePropertyChanged(() => AirlineName);

            PickupPoints = new List<PointsItems>();
            PickupPoints.Add(new PointsItems { Name = PUCurbSide, Fee = string.Empty });

            _PUPoints = new List<ListItem>();
            for( int iIndex = 0; iIndex < PickupPoints.Count; iIndex++ )
            {
                PointsItems pItem = PickupPoints[iIndex];
                _PUPoints.Add( new ListItem { Display = pItem.Name, Id = iIndex });

            }
            PUPointsId = 0;
            RaisePropertyChanged(() => PUPointsName);
        }

        private List<ListItem> _PUPoints;
        public List<ListItem> PUPoints
        {
            get
            {
                return _PUPoints;
            }
            set
            {
                _airlines = value ?? new List<ListItem>();
                RaisePropertyChanged();
                RaisePropertyChanged(() => PUPointsId);
                RaisePropertyChanged(() => PUPointsName);

            }
        }

        private int _PUPointsId;
        public int PUPointsId
        {
            get
            {
                return _PUPointsId;
            }
            set
            {
                if (value != _PUPointsId)
                {
                    _PUPointsId = value;
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

                var vehicle = PUPoints.FirstOrDefault(x => x.Id == PUPointsId);
                if (vehicle == null)
                {
                    vehicle = PUPoints.FirstOrDefault();
                }
                return vehicle.Display;
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

        private int? _AirLineId;
        public int? AirlineId
        {
            get
            {
                return _AirLineId;
            }
            set
            {
                if (value != _AirLineId)
                {
                    _AirLineId = value;
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
                return vehicle.Display;
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

        
        private String _FlightNum;
        public String FlightNum
        {
            get { return _FlightNum; }
            set
            {
                if (value != _FlightNum)
                {
                    _FlightNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        public String Title
        {
            get { return _pickupAddress.FullAddress; }
        }


        private void DateUpdated(DateTime? date)
        {
            PickupTimeStamp = date.HasValue
                ? date.Value.ToShortDateString() + " " + date.Value.ToShortTimeString()
                : this.Services().Localize["TimeNow"];
        }

        private string _PickupTimeStamp;
        public string PickupTimeStamp
        {
            get { return _PickupTimeStamp; }
            set
            {
                if (value != _PickupTimeStamp)
                {
                    _PickupTimeStamp = value;
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
                if (value != _pickupAddress)
                {
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
        }

        private string _POIPickup;
        public String POIPickup
        {
            get { return _POIPickup; }
            set
            {
                if (value != _POIPickup && value != null )
                {
                    _POIPickup = value;
                    if (value != string.Empty)
                    {
                        var pArray = JArray.Parse(value);

                        PickupPoints.Clear();
                        foreach (var pItem in pArray)
                        {
                            string sFee = string.Empty;
                            string sName = string.Empty;
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
                        _PUPoints = new List<ListItem>();
                        for (int iIndex = 0; iIndex < PickupPoints.Count; iIndex++)
                        {
                            PointsItems pItem = PickupPoints[iIndex];
                            _PUPoints.Add(new ListItem { Display = pItem.Name, Id = iIndex });

                        }
                        PUPointsId = 0;
                        RaisePropertyChanged(() => PUPoints);
                        RaisePropertyChanged(() => PUPointsName);
                    }
                }
            }
        }
        
        private string _POIAirline;
        public String POIAirline
        {
            get { return _POIAirline; }
            set
            {
                if ( value != _POIAirline && value != null )
                {
                    _POIAirline = value;
                    if (value != string.Empty)
                    {
                        var pArray = JArray.Parse(value);

                        _airlines.Clear();
                        int index = 0;
                        foreach (var pItem in pArray)
                        {
                            bool addItem = false;
                            string name = string.Empty;
                            foreach (var x in pItem)
                            {
                                if ((((JProperty)x).Name == "type") && (((string)((JProperty)x).Value).IndexOf("airline") != -1 ))
                                {
                                    addItem = true;
                                }
                                else if (((JProperty)x).Name == "name")
                                {
                                    name = ((string)((JProperty)x).Value);
                                }
                            }
                            if (addItem == true)
                            {
                                _airlines.Add(new ListItem { Display = name, Id = index++ });
                            }
                        }
                        AirlineId = 0;
                        RaisePropertyChanged(() => Airlines);
                        RaisePropertyChanged(() => AirlineName);
                    }
                }
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    // check if additional fee is accepted
                    PointsItems pItem = PickupPoints[PUPointsId];
                    bool accepted = true;
                    int fee = 0;
                    if (pItem != null && pItem.Fee != string.Empty)
                    {
						foreach( char c in pItem.Fee )
						{
							if( Char.IsDigit(c)== true )
							{
								fee *= 10;
                                // When converting, subtract 0x30 to get true value between 0 - 9
								fee += Convert.ToInt32(c - 0x30);
							}
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
                return this.GetCommand(() =>
                {
					((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.AirportPickDate;
                });
            }
        }
    }
}