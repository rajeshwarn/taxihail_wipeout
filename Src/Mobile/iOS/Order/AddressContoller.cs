//using System;
//using System.Collections.Generic;
//using System.Linq;
//using MonoTouch.Foundation;
//using MonoTouch.UIKit;
//using MonoTouch.MapKit;
//using MonoTouch.CoreLocation;
//using apcurium.Framework.Extensions;
//using TinyIoC;
//using apcurium.MK.Booking.Api.Contract.Resources;
//using apcurium.MK.Booking.Mobile.AppServices;
//using apcurium.MK.Booking.Mobile.Extensions;
//using System.Drawing;
//using apcurium.MK.Booking.Mobile.Client.Controls;
//using System.Threading;
//
//namespace apcurium.MK.Booking.Mobile.Client
//{
//    public class AddressContoller
//    {
//		private AddressBar _addressBar;
//        private UITableView _table;
//        private VerticalButtonBar _bar;
//        private SimilarAddressTableDatasource _similarDatasource;
//        private SimilarAddressTableDelegate _similarDelegate;
//        private Func<Address> _getLocation;
//        private Func<bool> _isEnabled;
//        private MKMapView _map;
//        private Action<Address> _setLocation;
//        private AddressAnnotationType _adrsType;
//        private string _mapTitle;
//        private UITextField _apt;
//        private UITextField _ringCode;
//        private bool _regionChangedSuspended;
//        private UserTouchedGesture _gesture;
//
//        public event EventHandler LocationHasChanged;
//
//        public AddressContoller(AddressBar addressBar, UITableView table, MKMapView map, AddressAnnotationType adrsType, 
//                                  Func<Address> getLocation, Action<Address> setLocation )
//        {
////            _apt = apt;
////            _ringCode = ringCode;
//            _map = map;
//            _adrsType = adrsType;
//            _table = table;
//			_addressBar = addressBar;
//            _getLocation = getLocation;
//            _setLocation = setLocation;
//
////			_addressBar.Started -= StartAddressEdit;
////			_addressBar.Started += StartAddressEdit;
////            
////			_addressBar.Ended -= SearchAddress;
////			_addressBar.Ended += SearchAddress;
////            
////			_addressBar.EditingChanged -= AddressChanged;
////			_addressBar.EditingChanged += AddressChanged;
////
////			_addressBar.BarItemClicked -= PickAddressTouchUpInside;
////			_addressBar.BarItemClicked += PickAddressTouchUpInside;
//            
//            _similarDelegate = new SimilarAddressTableDelegate(adrs => SetLocation(adrs, true, true));
//            _similarDatasource = new SimilarAddressTableDatasource();
//            
//            _table.Delegate = _similarDelegate;
//            _table.DataSource = _similarDatasource;
//            _table.RowHeight = 45;
//            _table.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
//            _table.SeparatorColor = UIColor.FromRGB(0.9f, 0.9f, 0.9f);
//            
//        }
//
//        public void StartTrackingMapMoving()
//        {
//            _gesture = new UserTouchedGesture();
//            
//            _map.AddGestureRecognizer(_gesture);
//            
//            _map.RegionChanged += delegate
//            {
//                
//                try
//                {
//                    if (_gesture.GetLastTouchDelay() < 1000)
//                    {
//                    
//                        Console.WriteLine("RegionChanged!!!");
//                        Console.WriteLine("LA:" + _map.CenterCoordinate.Latitude.ToString());
//                        Console.WriteLine("LO:" + _map.CenterCoordinate.Longitude.ToString());
//                        LoadAddress(_map.CenterCoordinate.Latitude, _map.CenterCoordinate.Longitude, false, false);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Logger.LogError(ex);
//                }
//                
//            };
//        }
//
//        void StartAddressEdit(object sender, EventArgs e)
//        {
//			//          _btn.Hidden = _addressBar.Text.HasValue ();
//        }
//
//        void AddressChanged(object sender, EventArgs e)
//        {
//			//          _btn.Hidden = _addressBar.Text.HasValue ();
//            
//            Address[] similars = new Address[0];
//			if (_addressBar.Text.HasValue())
//            {
//                var service = TinyIoCContainer.Current.Resolve<IBookingService>();
//
//                //TODO : Fix this
//				//                similars = service.FindSimilar(_addressBar.Text);
////                
////                _similarDelegate.Similars = similars;
////                _similarDatasource.Similars = similars;
////                _table.ReloadData();
//                
//            }
//            Address data = _getLocation();
//            data.Longitude = 0;
//            data.Latitude = 0;
//            _table.Hidden = similars.Count() == 0;
//        }
//
//        public void AssignData()
//        {
//            Address data = _getLocation();
//            
//			_addressBar.Text = data.FullAddress;
//            
//            
//            _apt.Maybe(() =>
//            {
//                _apt.Text = "";
//                _apt.Text = data.Apartment;
//            }
//            );
//            
//            
//            
//            _ringCode.Maybe(() =>
//            {
//                _ringCode.Text = "";
//                _ringCode.Text = data.RingCode;
//            }
//            );
//            
//        }
//
//        public void PrepareData()
//        {
//            Address data = _getLocation();
//            
//			data.FullAddress = _addressBar.Text;
//            
//            data.Apartment = "";
//            _apt.Maybe(() => data.Apartment = _apt.Text);
//            
//            data.RingCode = "";
//            _ringCode.Maybe(() => data.RingCode = _ringCode.Text);
//            
//            if (data.FullAddress.HasValue())
//            {
//                if (data.Longitude == 0 || data.Latitude == 0)
//                {
//                    SearchAddress(this, EventArgs.Empty);
//                }
//            }
//            
//        }
//        
//        LocationsTabView _adrsSelector;
//
//        void PickAddressTouchUpInside(int index)
//        {
//
//            switch (index)
//            {
//                case 0:
//                    ShowCurrentLocation(true);
//                    break;
//                case 1:
//                //_disableAutoSearch = true;
//                    _adrsSelector = new LocationsTabView();
//                    _adrsSelector.Mode = LocationsTabViewMode.FavoritesSelector;
//                    _adrsSelector.LocationSelected += LocationSelectedDelegate;
//                    _adrsSelector.Canceled += CancelSelectionDelegate;
//                    AppContext.Current.Controller.TopViewController.NavigationController.PushViewController(_adrsSelector, true);
//                    break;
//                case 2:
//                    var contactPicker = new ContactPicker(AppContext.Current.Controller.TopViewController);
//                    contactPicker.AddProperty(MonoTouch.AddressBook.ABPersonProperty.Address);
//                    contactPicker.ContactSelected += HandleContactSelected;
//                    contactPicker.Show();
//                    break;
//                case 3:
//                    var places = TinyIoCContainer.Current.Resolve<IGoogleService>().GetNearbyPlaces(_map.CenterCoordinate.Latitude, _map.CenterCoordinate.Longitude);
//                    _adrsSelector = new LocationsTabView();
//                    _adrsSelector.Mode = LocationsTabViewMode.NearbyPlacesSelector;
//                    _adrsSelector.LocationList = places.ToList();
//                    _adrsSelector.LocationSelected += LocationSelectedDelegate;
//                    _adrsSelector.Canceled += CancelSelectionDelegate;
//                    AppContext.Current.Controller.TopViewController.NavigationController.PushViewController(_adrsSelector, true);
//                    break;
//            }
//        }
//
//        void HandleContactSelected(object sender, ContactPickerResult e)
//        {
//			_addressBar.Text = e.Value;
//            SearchAddress(this, EventArgs.Empty);
//        }
//
//        private void LocationSelectedDelegate(object sender, EventArgs e)
//        {
//            DoLocationChange(_adrsSelector.SelectedLocation.Copy());
//        }
//
//        private void DoLocationChange(Address locData)
//        {
//            try
//            {
//                AppContext.Current.Controller.TopViewController.NavigationController.PopViewControllerAnimated(true);
//
//                SetLocation(locData, true, true);
//
//                var locationData = _getLocation();
//                if (locationData.Longitude == 0 || locationData.Latitude == 0)
//                {
//                    SearchAddress(this, EventArgs.Empty);
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex);
//            }
//        }
//
//        private void CancelSelectionDelegate(object sender, EventArgs e)
//        {
//            try
//            {
//                AppContext.Current.Controller.TopViewController.NavigationController.PopViewControllerAnimated(true); 
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex);
//            }
//        }
//
//        void SearchAddress(object sender, EventArgs e)
//        {
//            
//            SuspendRegionChanged();
//            
//            try
//            {
//                Console.WriteLine(_map.Annotations.Count().ToString());
//            
//				if (_addressBar.Text.IsNullOrEmpty())
//                {
//                    ShowCurrentLocation(true);
//                }
//                else
//                {
////                  _btn.Hidden = false;
//                    _table.Hidden = true;
//                    var service = TinyIoCContainer.Current.Resolve<IGeolocService>();
//					var result = service.ValidateAddress(_addressBar.Text);
//                
//                    
//                    if (result != null)
//                    {
//                        SetLocation(result, false, true);
//                    }
////                    else if (result.Count() > 0)
////                    {
////                    
////                        _similarDelegate.Similars = result;
////                        _similarDatasource.Similars = result;
////                        _table.ReloadData();
////                        _table.Hidden = false;
////                    
////                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex);
//            }
//            finally
//            {
//                ResumeRegionChanged();
//            }
//            
//        }
//
//        private void SetLocation(Address locationData, bool overrideAptRingCode, bool setRegionOnMap)
//        {
//            try
//            { 
//				_addressBar.InvokeOnMainThread(() =>
//                {
//					_addressBar.EditingChanged -= AddressChanged;
//					_addressBar.ResignFirstResponder();
//                
//                    _table.Hidden = true;
//                    _setLocation(locationData.Copy());
//					_addressBar.Text = locationData.FullAddress;
//            
//                    if (overrideAptRingCode)
//                    {
//                        _apt.Maybe(() => _apt.InvokeOnMainThread(() => _apt.Text = locationData.Apartment));
//                        _ringCode.Maybe(() => _ringCode.InvokeOnMainThread(() => _ringCode.Text = locationData.RingCode));
//                    }
//            
//                    if ((_map.Annotations != null) && (_map.Annotations.OfType<AddressAnnotation>().Count(a => a.AddressType == _adrsType) > 0))
//                    {
//                        _map.RemoveAnnotations(_map.Annotations.OfType<AddressAnnotation>().Where(a => a.AddressType == _adrsType).ToArray());
//                    }
//            
//                
//                    SuspendRegionChanged();
//                    try
//                    {
//                        var coordinate = _getLocation().GetCoordinate();
//                        if (setRegionOnMap)
//                        {
//                            _map.SetRegion(new MKCoordinateRegion(coordinate, new MKCoordinateSpan(0.02, 0.02)), true);
//                        }
//                        _map.AddAnnotation(new AddressAnnotation(coordinate, _adrsType, _mapTitle, _getLocation().FullAddress));
//                    }
//                    finally
//                    {
//                        ResumeRegionChanged();
//                    }
//                    
//					_addressBar.EditingChanged += AddressChanged;
//                
//            
//                
//                    if (LocationHasChanged != null)
//                    {
//                        LocationHasChanged(this, EventArgs.Empty);
//                    }
//                }
//                );
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex);
//            }   
//        }
//
//        public void SuspendRegionChanged()
//        {
//            _regionChangedSuspended = true;
//        }
//
//        public void ResumeRegionChanged()
//        {
//            _regionChangedSuspended = false;
//        }
//
//        private bool _showCurrentLocationCanceled = false;
//
//        //private CLLocationManager _locationManager;
//		public void CenterMapOnUserLocation ()
//		{
//           // .
////			_locationManager= new CLLocationManager();
////			_locationManager.UpdatedLocation += (object sender, CLLocationUpdatedEventArgs e) => {
////				_locationManager.StopUpdatingLocation();
////				_map.SetRegion(new MKCoordinateRegion(e.NewLocation.Coordinate, new MKCoordinateSpan(0.02, 0.02)), true);
////			};
////			_locationManager.StartUpdatingLocation();
//
//                _map.SetRegion( new MKCoordinateRegion(AppContext.Current.CurrrentLocation.Coordinate , new MKCoordinateSpan(0.02, 0.02)), true);
//		}
//
//		public void RefreshData()
//		{
//			ShowCurrentLocation(AppContext.Current.LoggedUser != null);
//		}
//
//        public void ShowCurrentLocation(bool showProgress)
//        {
//            if ((AppContext.Current.LoggedUser == null) || (!(AppContext.Current.Controller.TopViewController is UINavigationController)) || (!(((UINavigationController)AppContext.Current.Controller.TopViewController).TopViewController is BookView)))
//            {
//                return;
//            }
//
//            _showCurrentLocationCanceled = false;
//            if (showProgress)
//            {
//				LoadingOverlay.StartAnimatingLoading(_addressBar, LoadingOverlayPosition.Center, Resources.Locating, 130, 30, () => _showCurrentLocationCanceled = true);
//            }
//            
//            
//            ThreadHelper.ExecuteInThread(() =>
//            {
//                
//                try
//                {                                       
//
//                    int count = 0;
//                    while ((count < 30) && ((AppContext.Current.CurrrentLocation.HorizontalAccuracy > 100) || (AppContext.Current.CurrrentLocation.Coordinate.Longitude == 0)))
//                    {
//                        Console.WriteLine("Finding position" + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString());
//                        System.Threading.Thread.Sleep(100);
//                        count++;
//                    }
//                                        
//                    Logger.LogMessage("GPS located : " + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString() + " (" + count.ToString() + ")");
//                                        
//                                  
//                    Logger.LogMessage("GPS located CLat: " + AppContext.Current.CurrrentLocation.Coordinate.Latitude.ToString());
//                    Logger.LogMessage("GPS located CLong: " + AppContext.Current.CurrrentLocation.Coordinate.Longitude.ToString());
//                    Logger.LogMessage("GPS located CAcc: " + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString());
//                                        
//                                        
//                     
//                    if (!_showCurrentLocationCanceled)
//                    {        
//                        LoadAddress(AppContext.Current.CurrrentLocation.Coordinate.Latitude, AppContext.Current.CurrrentLocation.Coordinate.Longitude, (count >= 39), true);
//                    }
//                        
//                
//                }
//                catch (Exception ex)
//                {
//                    Logger.LogError(ex);
//                }
//                finally
//                {
//                    _showCurrentLocationCanceled = false;
//                    if (showProgress)
//                    {
//						_addressBar.InvokeOnMainThread(() => {
//							LoadingOverlay.StopAnimatingLoading(_addressBar); }
//                        );
//                    }
//                }
//            }
//            );
//        }
//
//        private void LoadAddress(double latitude, double longitude, bool isGPSNotAccurate, bool setRegionOnMap)
//        {
//            var addresses = TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(latitude, longitude);
//               
//            if (addresses.Count() == 1)
//            {                    
//                SetLocation(addresses[0], true, setRegionOnMap);                        
//            }
//        }
//        
//        
//        
//    }
//}
//
