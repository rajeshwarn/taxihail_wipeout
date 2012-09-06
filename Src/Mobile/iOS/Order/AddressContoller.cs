using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class AddressContoller
    {
		private UITextField _text;
        private UITableView _table;
        private VerticalButtonBar _bar;
        private SimilarAddressTableDatasource _similarDatasource;
        private SimilarAddressTableDelegate _similarDelegate;
        private Func<Address> _getLocation;
        private Func<bool> _isEnabled;
        private MKMapView _map;
        private Action<Address> _setLocation;
        private AddressAnnotationType _adrsType;
        private string _mapTitle;
        private UITextField _apt;
        private UITextField _ringCode;
        private bool _regionChangedSuspended;
        private UserTouchedGesture _gesture;

        public event EventHandler LocationHasChanged;

        public AddressContoller(UITextField text, UITextField apt, UITextField ringCode, UITableView table, MKMapView map, AddressAnnotationType adrsType, 
                                 string mapTitle, Func<Address> getLocation, Action<Address> setLocation, Func<bool> isEnabled)
        {
            _isEnabled = isEnabled;
            _mapTitle = mapTitle;
            _apt = apt;
            _ringCode = ringCode;
            _map = map;
            _adrsType = adrsType;
            _table = table;
            _text = text;
            _getLocation = getLocation;
            _setLocation = setLocation;
            _text.Started -= StartAddressEdit;
            _text.Started += StartAddressEdit;
            
            _text.Ended -= SearchAddress;
            _text.Ended += SearchAddress;
            
            _text.EditingChanged -= AddressChanged;
            _text.EditingChanged += AddressChanged;
            
            _text.ReturnKeyType = UIReturnKeyType.Search;
            _text.AutocorrectionType = UITextAutocorrectionType.No;
            _text.AutocapitalizationType = UITextAutocapitalizationType.None;
            _text.ShouldReturn = delegate(UITextField textField)
            {
                return _text.ResignFirstResponder();
            };


            
            _bar = new VerticalButtonBar(new RectangleF(_text.Frame.Right + 4, _text.Frame.Top, 39f, 34f));
            _bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/locationIcon.png"), UIImage.FromFile("Assets/VerticalButtonBar/locationIcon.png"));
            //vertBar.AddButton( UIImage.FromFile("Assets/VerticalButtonBar/targetIcon.png" ), UIImage.FromFile("Assets/VerticalButtonBar/targetIcon.png" ) );
            _bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/favoriteIcon.png"), UIImage.FromFile("Assets/VerticalButtonBar/favoriteIcon.png"));
            _bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/contacts.png"), UIImage.FromFile("Assets/VerticalButtonBar/contacts.png"));
            _bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/nearbyIcon.png"), UIImage.FromFile("Assets/VerticalButtonBar/nearbyIcon.png"));
			_text.Superview.AddSubview(_bar);


            
            _bar.ButtonClicked -= PickAddressTouchUpInside;
			_bar.ButtonClicked += PickAddressTouchUpInside;
            
            _similarDelegate = new SimilarAddressTableDelegate(adrs => SetLocation(adrs, true, true));
            _similarDatasource = new SimilarAddressTableDatasource();
            
            _table.Delegate = _similarDelegate;
            _table.DataSource = _similarDatasource;
            _table.RowHeight = 45;
            _table.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            _table.SeparatorColor = UIColor.FromRGB(0.9f, 0.9f, 0.9f);
            
            UIImageView img = new UIImageView(UIImage.FromFile("Assets/location.png"));
            img.BackgroundColor = UIColor.Clear;
            img.Frame = new System.Drawing.RectangleF(_map.Frame.X + ((_map.Frame.Width / 2) - 10), _map.Frame.Y + ((_map.Frame.Height / 2)) - 30, 20, 20);
            map.Superview.AddSubview(img);
            _map.MultipleTouchEnabled = true;
            
                

            
        }

        public void StartTrackingMapMoving()
        {
            _gesture = new UserTouchedGesture();
            
            _map.AddGestureRecognizer(_gesture);
            
            _map.RegionChanged += delegate
            {
                
                try
                {
                    if (_gesture.GetLastTouchDelay() < 1000)
                    {
                    
                        Console.WriteLine("RegionChanged!!!");
                        Console.WriteLine("LA:" + _map.CenterCoordinate.Latitude.ToString());
                        Console.WriteLine("LO:" + _map.CenterCoordinate.Longitude.ToString());
                        LoadAddress(_map.CenterCoordinate.Latitude, _map.CenterCoordinate.Longitude, false, false);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
                
            };
        }

        void StartAddressEdit(object sender, EventArgs e)
        {
//          _btn.Hidden = _text.Text.HasValue ();
        }

        void AddressChanged(object sender, EventArgs e)
        {
//          _btn.Hidden = _text.Text.HasValue ();
            
            Address[] similars = new Address[0];
            if (_text.Text.HasValue())
            {
                var service = TinyIoCContainer.Current.Resolve<IBookingService>();

                //TODO : Fix this
//                similars = service.FindSimilar(_text.Text);
//                
//                _similarDelegate.Similars = similars;
//                _similarDatasource.Similars = similars;
//                _table.ReloadData();
                
            }
            Address data = _getLocation();
            data.Longitude = 0;
            data.Latitude = 0;
            _table.Hidden = similars.Count() == 0;
        }

        public void AssignData()
        {
            Address data = _getLocation();
            
            _text.Text = data.FullAddress;
            
            
            _apt.Maybe(() =>
            {
                _apt.Text = "";
                _apt.Text = data.Apartment;
            }
            );
            
            
            
            _ringCode.Maybe(() =>
            {
                _ringCode.Text = "";
                _ringCode.Text = data.RingCode;
            }
            );
            
        }

        public void PerpareData()
        {
            Address data = _getLocation();
            
            data.FullAddress = _text.Text;
            
            data.Apartment = "";
            _apt.Maybe(() => data.Apartment = _apt.Text);
            
            data.RingCode = "";
            _ringCode.Maybe(() => data.RingCode = _ringCode.Text);
            
            if (data.FullAddress.HasValue())
            {
                if (data.Longitude == 0 || data.Latitude == 0)
                {
                    SearchAddress(this, EventArgs.Empty);
                }
            }
            
        }
        
        LocationsTabView _adrsSelector;

        void PickAddressTouchUpInside(int index)
        {

            switch (index)
            {
                case 0:
                    ShowCurrentLocation(true);
                    break;
                case 1:
                //_disableAutoSearch = true;
                    _adrsSelector = new LocationsTabView();
                    _adrsSelector.Mode = LocationsTabViewMode.FavoritesSelector;
                    _adrsSelector.LocationSelected += LocationSelectedDelegate;
                    _adrsSelector.Canceled += CancelSelectionDelegate;
                    AppContext.Current.Controller.SelectedUIViewController.NavigationController.PushViewController(_adrsSelector, true);
                    break;
                case 2:
                    var contactPicker = new ContactPicker(AppContext.Current.Controller.SelectedUIViewController);
                    contactPicker.AddProperty(MonoTouch.AddressBook.ABPersonProperty.Address);
                    contactPicker.ContactSelected += HandleContactSelected;
                    contactPicker.Show();
                    break;
                case 3:
                    var places = TinyIoCContainer.Current.Resolve<IGoogleService>().GetNearbyPlaces(_map.CenterCoordinate.Latitude, _map.CenterCoordinate.Longitude);
                    _adrsSelector = new LocationsTabView();
                    _adrsSelector.Mode = LocationsTabViewMode.NearbyPlacesSelector;
                    _adrsSelector.LocationList = places.ToList();
                    _adrsSelector.LocationSelected += LocationSelectedDelegate;
                    _adrsSelector.Canceled += CancelSelectionDelegate;
                    AppContext.Current.Controller.SelectedUIViewController.NavigationController.PushViewController(_adrsSelector, true);
                    break;
            }
        }

        void HandleContactSelected(object sender, ContactPickerResult e)
        {
            _text.Text = e.Value;
            SearchAddress(this, EventArgs.Empty);
        }

        private void LocationSelectedDelegate(object sender, EventArgs e)
        {
            DoLocationChange(_adrsSelector.SelectedLocation.Copy());
        }

        private void DoLocationChange(Address locData)
        {
            try
            {
                AppContext.Current.Controller.SelectedUIViewController.NavigationController.PopViewControllerAnimated(true);

                SetLocation(locData, true, true);

                var locationData = _getLocation();
                if (locationData.Longitude == 0 || locationData.Latitude == 0)
                {
                    SearchAddress(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void CancelSelectionDelegate(object sender, EventArgs e)
        {
            try
            {
                AppContext.Current.Controller.SelectedUIViewController.NavigationController.PopViewControllerAnimated(true); 
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        void SearchAddress(object sender, EventArgs e)
        {
            
            SuspendRegionChanged();
            
            try
            {
                Console.WriteLine(_map.Annotations.Count().ToString());
            
                if (_text.Text.IsNullOrEmpty())
                {
                    ShowCurrentLocation(true);
                }
                else
                {
//                  _btn.Hidden = false;
                    _table.Hidden = true;
                    var service = TinyIoCContainer.Current.Resolve<IGeolocService>();
                    var result = service.ValidateAddress(_text.Text);
                
                    
                    if (result != null)
                    {
                        SetLocation(result, false, true);
                    }
//                    else if (result.Count() > 0)
//                    {
//                    
//                        _similarDelegate.Similars = result;
//                        _similarDatasource.Similars = result;
//                        _table.ReloadData();
//                        _table.Hidden = false;
//                    
//                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                ResumeRegionChanged();
            }
            
        }

        private void SetLocation(Address locationData, bool overrideAptRingCode, bool setRegionOnMap)
        {
            try
            {
                
                if (!_isEnabled())
                {
                    return;
                }
            
                _text.InvokeOnMainThread(() =>
                {
                    
                    _text.EditingChanged -= AddressChanged;
                    _text.ResignFirstResponder();
                
                    _table.Hidden = true;
                    _setLocation(locationData.Copy());
                    _text.Text = locationData.FullAddress;
            
                    if (overrideAptRingCode)
                    {
                        _apt.Maybe(() => _apt.InvokeOnMainThread(() => _apt.Text = locationData.Apartment));
                        _ringCode.Maybe(() => _ringCode.InvokeOnMainThread(() => _ringCode.Text = locationData.RingCode));
                    }
            
                    if ((_map.Annotations != null) && (_map.Annotations.OfType<AddressAnnotation>().Count(a => a.AddressType == _adrsType) > 0))
                    {
                        _map.RemoveAnnotations(_map.Annotations.OfType<AddressAnnotation>().Where(a => a.AddressType == _adrsType).ToArray());
                    }
            
                
                    SuspendRegionChanged();
                    try
                    {
                        var coordinate = _getLocation().GetCoordinate();
                        if (setRegionOnMap)
                        {
                            _map.SetRegion(new MKCoordinateRegion(coordinate, new MKCoordinateSpan(0.02, 0.02)), true);
                        }
                        _map.AddAnnotation(new AddressAnnotation(coordinate, _adrsType, _mapTitle, _getLocation().FullAddress));
                    }
                    finally
                    {
                        ResumeRegionChanged();
                    }
                    
                    _text.EditingChanged += AddressChanged;
                
            
                
                    if (LocationHasChanged != null)
                    {
                        LocationHasChanged(this, EventArgs.Empty);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
                
            
        }

        public void SuspendRegionChanged()
        {
            _regionChangedSuspended = true;
        }

        public void ResumeRegionChanged()
        {
            _regionChangedSuspended = false;
        }

        private bool _showCurrentLocationCanceled = false;

        //private CLLocationManager _locationManager;
		public void CenterMapOnUserLocation ()
		{
           // .
//			_locationManager= new CLLocationManager();
//			_locationManager.UpdatedLocation += (object sender, CLLocationUpdatedEventArgs e) => {
//				_locationManager.StopUpdatingLocation();
//				_map.SetRegion(new MKCoordinateRegion(e.NewLocation.Coordinate, new MKCoordinateSpan(0.02, 0.02)), true);
//			};
//			_locationManager.StartUpdatingLocation();

                _map.SetRegion( new MKCoordinateRegion(AppContext.Current.CurrrentLocation.Coordinate , new MKCoordinateSpan(0.02, 0.02)), true);
		}

        public void ShowCurrentLocation(bool showProgress)
        {
            
            
            if ((AppContext.Current.LoggedUser == null) || (!(AppContext.Current.Controller.SelectedViewController is UINavigationController)) || (!(((UINavigationController)AppContext.Current.Controller.SelectedViewController).TopViewController is BookTabView)))
            {
                return;
            }
            
            
            _showCurrentLocationCanceled = false;
            if (showProgress)
            {
                LoadingOverlay.StartAnimatingLoading(_text, LoadingOverlayPosition.Center, Resources.Locating, 130, 30, () => _showCurrentLocationCanceled = true);
            }
            
            
            ThreadHelper.ExecuteInThread(() =>
            {
                
                try
                {                                       

                    int count = 0;
                    while ((count < 30) && ((AppContext.Current.CurrrentLocation.HorizontalAccuracy > 100) || (AppContext.Current.CurrrentLocation.Coordinate.Longitude == 0)))
                    {
                        Console.WriteLine("Finding position" + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString());
                        System.Threading.Thread.Sleep(100);
                        count++;
                    }
                                        
                    Logger.LogMessage("GPS located : " + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString() + " (" + count.ToString() + ")");
                                        
                                  
                    Logger.LogMessage("GPS located CLat: " + AppContext.Current.CurrrentLocation.Coordinate.Latitude.ToString());
                    Logger.LogMessage("GPS located CLong: " + AppContext.Current.CurrrentLocation.Coordinate.Longitude.ToString());
                    Logger.LogMessage("GPS located CAcc: " + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString());
                                        
                                        
                     
                    if (!_showCurrentLocationCanceled)
                    {        
                        LoadAddress(AppContext.Current.CurrrentLocation.Coordinate.Latitude, AppContext.Current.CurrrentLocation.Coordinate.Longitude, (count >= 39), true);
                    }
                        
                
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
                finally
                {
                    _showCurrentLocationCanceled = false;
                    if (showProgress)
                    {
                        _text.InvokeOnMainThread(() => {
                            LoadingOverlay.StopAnimatingLoading(_text); }
                        );
                    }
                }
            }
            );
        }

        private void LoadAddress(double latitude, double longitude, bool isGPSNotAccurate, bool setRegionOnMap)
        {
            var addresses = TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(latitude, longitude);
               
            if (addresses.Count() == 1)
            {                    
                SetLocation(addresses[0], true, setRegionOnMap);                        
            }
        }
        
        
        
    }
}

