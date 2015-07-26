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
	public class OrderAirportViewModel : BaseViewModel, IRequestPresentationState<HomeViewModelStateRequestedEventArgs>
	{
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IAccountService _accountService;
        private readonly IPaymentService _paymentService;

        public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;
#region Const and ReadOnly
        private const string NoAirlines = "No Airline";

        private const string PUCurbSide = "Curb Side";

#endregion

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
//            ChargeTypes = (await _accountService.GetPaymentsList()).Select(x => new ListItem { Id = x.Id, Display = this.Services().Localize[x.Display] }).ToArray();

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

        int? iPUPointsId;
        public int? PUPointsId
        {
            get
            {
                return iPUPointsId;
            }
            set
            {
                if (value != iPUPointsId)
                {
                    iPUPointsId = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(() => PUPointsName);
                    //					_orderWorkflowService.SetVehicleType (value);
                }
            }
        }

        public string PUPointsName
        {
            get
            {
                if (!PUPointsId.HasValue)
                {
                    return this.Services().Localize["NoPreference"];
                }

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

        int? iAirLineId;
        public int? AirlineId
        {
            get
            {
                return iAirLineId;
            }
            set
            {
                if (value != iAirLineId)
                {
                    iAirLineId = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(() => AirlineName);
                    //					_orderWorkflowService.SetVehicleType (value);
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

        
        private String sFlightNum;
        public String FlightNum
        {
            get { return sFlightNum; }
            set
            {
                if (value != sFlightNum)
                {
                    sFlightNum = value;
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

        private string sPickupTimeStamp;
        public string PickupTimeStamp
        {
            get { return sPickupTimeStamp; }
            set
            {
                if (value != sPickupTimeStamp)
                {
                    sPickupTimeStamp = value;
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
                        _orderWorkflowService.POIRefAirLineList(this.Services().Settings.TaxiHail.ApplicationKey, string.Empty, 0);
                        _orderWorkflowService.POIRefPickupList(this.Services().Settings.TaxiHail.ApplicationKey, string.Empty, 0);

                        // Clear/default any previous data
                        AirlineId = 0;
                        RaisePropertyChanged(() => AirlineName);
                        FlightNum = string.Empty;
                        PUPointsId = 0;
                        RaisePropertyChanged(() => PUPointsName);
                        //                    Log.Debug("MK-Dbg", String.Format("OrderAirportViewModel::PickupAddress: changed"));
                    }
                }
            }
        }

        private string objPOIPickup;
        public String POIPickup
        {
            get { return objPOIPickup; }
            set
            {
                if ((value != objPOIPickup)&&( value != null ))
                {
                    objPOIPickup = value;
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
//                            Log.Debug("MK-Dbg", String.Format("OrderAirportViewModel::POIPickup: {0}, fee={1}", sName, dFee));
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
        
        private string objPOIAirline;
        public String POIAirline
        {
            get { return objPOIAirline; }
            set
            {
                if ((value != objPOIAirline)&&( value != null ))
                {
                    objPOIAirline = value;
                    if (value != string.Empty)
                    {
//                        Log.Debug("MK-Dbg", String.Format("OrderAirportViewModel::POIAirline: {0}", value));
                        var pArray = JArray.Parse(value);

                        _airlines.Clear();
                        int iId = 0;
                        foreach (var pItem in pArray)
                        {
                            bool bAdd = false;
                            string sName = string.Empty;
                            foreach (var x in pItem)
                            {
                                if ((((JProperty)x).Name == "type") && (((string)((JProperty)x).Value).IndexOf("airline") != -1 ))
                                {
                                    bAdd = true;
                                }
                                else if (((JProperty)x).Name == "name")
                                {
                                    sName = ((string)((JProperty)x).Value);
                                }
                            }
                            if (bAdd == true)
                            {
                                _airlines.Add(new ListItem { Display = sName, Id = iId++ });
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
//                    var bookingSettings = await _orderWorkflowService.GetAndObserveBookingSettings().Take(1).ToTask();
//                    var pickupAddress = await _orderWorkflowService.GetAndObservePickupAddress().Take(1).ToTask();

                    // check if additional fee is accepted
                    PointsItems pItem = PickupPoints[(int)PUPointsId];
                    bool bAccepted = true;
                    int iFee = 0;
                    if ((pItem != null) && (pItem.Fee != string.Empty))
                    {
						foreach( char c in pItem.Fee )
						{
							if( Char.IsDigit(c)== true )
							{
								iFee *= 10;
								iFee += Convert.ToInt32(c - 0x30);
//                                Log.Debug("MK-Dbg", String.Format("OrderAirportViewModel::NextCommand: Fee: {0}", iFee));
							}
						}
						if( iFee > 0 )
						{
	                        bAccepted = false;
	                        String sFeeWarning = string.Format(this.Services().Localize["BookingAirportPickupPointFee"], pItem.Name, pItem.Fee);
	                        await this.Services().Message.ShowMessage(
	                            this.Services().Localize["WarningTitle"],
	                            sFeeWarning,
	                            this.Services().Localize["OkButtonText"],
	                            () =>
	                            {
	                                bAccepted = true;
	                            },
	                            this.Services().Localize["Cancel"],
	                            () => { });
						}
                    }
                    if (bAccepted)
                    {
//                        BookingSettings = bookingSettings.Copy();
//                        PickupAddress = pickupAddress.Copy();

                        StringBuilder sb = new StringBuilder();
                        if( Note.Length > 0 )
                        {
                            sb.Append("{0}\n", Note );
                        }
                        sb.Append(this.Services().Localize["BookingAirportDetails"], _pickupAddress.FriendlyName, AirlineName, FlightNum, PUPointsName);
                        if ((pItem != null) && (pItem.Fee != string.Empty)&&( iFee > 0 ))
                        {
                            sb.Append(this.Services().Localize["BookingAirportDetailsFee"], pItem.Fee);
                        }
                        Note = sb.ToString();
                        PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Review));

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
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.AirportPickDate));
                });
            }
        }
    }
}

