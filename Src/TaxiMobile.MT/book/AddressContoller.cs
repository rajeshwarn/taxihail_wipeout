using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;

namespace TaxiMobileApp
{
	public class AddressContoller
	{
		private UITextField _text;
		private UITableView _table;
		private UIButton _btn;
		private SimilarAddressTableDatasource _similarDatasource;
		private SimilarAddressTableDelegate _similarDelegate;
		private Func<LocationData> _getLocation;
		private Func<bool> _isEnabled;
		private MKMapView _map;
		private Action<LocationData> _setLocation;
		private AddressAnnotationType _adrsType;
		private string _mapTitle;
		private UITextField _apt;
		private UITextField _ringCode;
		private bool _regionChangedSuspended;
		private UserTouchedGesture _gesture;

		public event EventHandler LocationHasChanged;

		public AddressContoller (UITextField text, UITextField apt, UITextField ringCode, UITableView table, UIButton btn, MKMapView map, AddressAnnotationType adrsType, 
		                         string mapTitle, Func<LocationData> getLocation, Action<LocationData> setLocation, Func<bool> isEnabled)
		{
			_isEnabled = isEnabled;
			_mapTitle = mapTitle;
			_apt = apt;
			_ringCode = ringCode;
			_btn = btn;
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
			_text.ShouldReturn = delegate(UITextField textField) {
				return _text.ResignFirstResponder (); };
			
			
			_btn.TouchUpInside -= PickAddressTouchUpInside;
			_btn.TouchUpInside += PickAddressTouchUpInside;
			
			_similarDelegate = new SimilarAddressTableDelegate (adrs => SetLocation (adrs, true, true));
			_similarDatasource = new SimilarAddressTableDatasource ();
			
			_table.Delegate = _similarDelegate;
			_table.DataSource = _similarDatasource;
			_table.RowHeight = 45;
			_table.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
			_table.SeparatorColor = UIColor.FromRGB (0.9f, 0.9f, 0.9f);
			
			
			
			
			_map.SetRegion (new MKCoordinateRegion (new CLLocationCoordinate2D (45.529, -73.630), new MKCoordinateSpan (0.1, 0.1)), true);
			
			
			UIImageView img = new UIImageView (UIImage.FromFile ("Assets/location.png"));
			img.BackgroundColor = UIColor.Clear;
			img.Frame = new RectangleF (_map.Frame.X + ((_map.Frame.Width / 2) - 10), _map.Frame.Y + ((_map.Frame.Height / 2) )-30, 20, 20);
			map.Superview.AddSubview (img);
			_map.MultipleTouchEnabled = true;
			
				
			_gesture = new UserTouchedGesture ();
			
			_map.AddGestureRecognizer (_gesture);
			
			_map.RegionChanged += delegate {
				
				try
				{
					if (_gesture.GetLastTouchDelay () < 1000)
					{
					
						Console.WriteLine ("RegionChanged!!!");
						Console.WriteLine ("LA:" + _map.CenterCoordinate.Latitude.ToString ());
						Console.WriteLine ("LO:" + _map.CenterCoordinate.Longitude.ToString ());
						LoadAddress (_map.CenterCoordinate.Latitude, _map.CenterCoordinate.Longitude, false, false);
					}
				}
				catch (Exception ex)
				{
					Logger.LogError (ex);
				}
				
			};
			
		}

		void StartAddressEdit (object sender, EventArgs e)
		{
			_btn.Hidden = _text.Text.HasValue ();
		}

		void AddressChanged (object sender, EventArgs e)
		{
			_btn.Hidden = _text.Text.HasValue ();
			
			LocationData[] similars = new LocationData[0];
			if (_text.Text.HasValue ())
			{
				var service = ServiceLocator.Current.GetInstance<IBookingService> ();
				similars = service.FindSimilar (_text.Text);
				
				_similarDelegate.Similars = similars;
				_similarDatasource.Similars = similars;
				_table.ReloadData ();
				
			}
			LocationData data = _getLocation ();
			data.Longitude = null;
			data.Latitude = null;
			_table.Hidden = similars.Count () == 0;
		}

		public void AssignData ()
		{
			LocationData data = _getLocation ();
			
			_text.Text = data.Address;
			
			
			_apt.Maybe (() =>
			{
				_apt.Text = "";
				_apt.Text = data.Apartment;
			});
			
			
			
			_ringCode.Maybe (() =>
			{
				_ringCode.Text = "";
				_ringCode.Text = data.RingCode;
			});
			
		}

		public void PerpareData ()
		{
			LocationData data = _getLocation ();
			
			data.Address = _text.Text;
			
			data.Apartment = "";
			_apt.Maybe (() => data.Apartment = _apt.Text);
			
			data.RingCode = "";
			_ringCode.Maybe (() => data.RingCode = _ringCode.Text);
			
			if (data.Address.HasValue ())
			{
				if (!data.Longitude.HasValue || !data.Latitude.HasValue)
				{
					SearchAddress (this, EventArgs.Empty);
				}
			}
			
		}
		
		LocationsTabView _adrsSelector;

		void PickAddressTouchUpInside (object sender, EventArgs e)
		{
			
			//_disableAutoSearch = true;
			_adrsSelector = new LocationsTabView ();
			_adrsSelector.Mode = LocationsTabViewMode.Selector;
			_adrsSelector.LocationSelected += delegate {
				
				try
				{
					AppContext.Current.Controller.SelectedUIViewController.NavigationController.PopViewControllerAnimated (true);
				
								
					SetLocation (_adrsSelector.SelectedLocation.Copy (), true, true);
					var locationData = _getLocation ();
					if (!locationData.Longitude.HasValue || !locationData.Latitude.HasValue)
					{
						SearchAddress (this, EventArgs.Empty);
					}
				}
				catch (Exception ex)
				{
					Logger.LogError (ex);
				}
				
			};
			
			_adrsSelector.Canceled += delegate {
				try
				{
					AppContext.Current.Controller.SelectedUIViewController.NavigationController.PopViewControllerAnimated (true); 
				}
				catch (Exception ex)
				{
					Logger.LogError (ex);
				}
			};
			
			AppContext.Current.Controller.SelectedUIViewController.NavigationController.PushViewController (_adrsSelector, true);
			
		}

		void SearchAddress (object sender, EventArgs e)
		{
			
			SuspendRegionChanged ();
			
			try
			{
				Console.WriteLine (_map.Annotations.Count ().ToString ());
			
				if (_text.Text.IsNullOrEmpty ())
				{
					ShowCurrentLocation (true);
				}
				else
				{
					_btn.Hidden = false;
					_table.Hidden = true;
					var service = ServiceLocator.Current.GetInstance<IBookingService> ();
					var result = service.SearchAddress (_text.Text);
				
					Console.WriteLine ("Adrs found : " + result.Count ().ToString ());
					
					if (result.Count () == 1)
					{
						Console.WriteLine ("Adrs found : " + result [0].Name);
						SetLocation (result [0], false, true);
					}
					else
					if (result.Count () > 0)
					{
					
						_similarDelegate.Similars = result;
						_similarDatasource.Similars = result;
						_table.ReloadData ();
						_table.Hidden = false;
					
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogError (ex);
			}
			finally
			{
				ResumeRegionChanged ();
			}
			
		}

		private void SetLocation (LocationData locationData, bool overrideAptRingCode, bool setRegionOnMap)
		{
			try
			{
				
				if (!_isEnabled ())
				{
					return;
				}
			
				_text.InvokeOnMainThread (() =>
				{
					
					_text.EditingChanged -= AddressChanged;
					_text.ResignFirstResponder ();
				
					_table.Hidden = true;
					_setLocation (locationData.Copy ());
					_text.Text = locationData.Address;
			
					if (overrideAptRingCode)
					{
						_apt.Maybe (() => _apt.InvokeOnMainThread (() => _apt.Text = locationData.Apartment));
						_ringCode.Maybe (() => _ringCode.InvokeOnMainThread (() => _ringCode.Text = locationData.RingCode));
					}
			
					if ((_map.Annotations != null) && (_map.Annotations.OfType<AddressAnnotation> ().Count (a => a.AddressType == _adrsType) > 0))
					{
						_map.RemoveAnnotations (_map.Annotations.OfType<AddressAnnotation> ().Where (a => a.AddressType == _adrsType).ToArray ());
					}
			
				
					SuspendRegionChanged ();
					try					{
						var coordinate = _getLocation ().GetCoordinate ();
						if (setRegionOnMap)
						{
							_map.SetRegion (new MKCoordinateRegion (coordinate, new MKCoordinateSpan (0.02, 0.02)), true);
						}
						_map.AddAnnotation (new AddressAnnotation (coordinate, _adrsType, _mapTitle, _getLocation ().Address));
					}
					finally					{
						ResumeRegionChanged ();
					}
					
					_text.EditingChanged += AddressChanged;
				
			
				
					if (LocationHasChanged != null)
					{
						LocationHasChanged (this, EventArgs.Empty);
					}
				});
			}
			catch (Exception ex)
			{
				Logger.LogError (ex);
			}
				
			
		}

		public void SuspendRegionChanged ()
		{
			_regionChangedSuspended = true;
		}

		public void ResumeRegionChanged ()
		{
			_regionChangedSuspended = false;
		}

		private bool _showCurrentLocationCanceled = false;

		public void ShowCurrentLocation (bool showProgress)
		{
			
			
			if ((!(AppContext.Current.Controller.SelectedViewController is UINavigationController)) || (!(((UINavigationController)AppContext.Current.Controller.SelectedViewController).TopViewController is BookTabView)))
			{
				return;
			}
			
			
			_showCurrentLocationCanceled = false;
			if (showProgress)
			{
				LoadingOverlay.StartAnimatingLoading (_text, LoadingOverlayPosition.Center, Resources.Locating, 130, 30, () => _showCurrentLocationCanceled = true);
			}
			
			
			ThreadHelper.ExecuteInThread (() =>
			{
				
				try
				{										
					
					 
                                        int count = 0;
                                        while ((count < 40) && ((AppContext.Current.CurrrentLocation.HorizontalAccuracy > 85) || (AppContext.Current.CurrrentLocation.Coordinate.Longitude == 0)))
                                        {
                                                Console.WriteLine ("Finding position" + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString ());
                                                Thread.Sleep (300);
                                                count++;
                                        }
                                        
                                        Logger.LogMessage ("GPS located : " + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString () + " (" + count.ToString () + ")");
                                        
                                        
                                        try
                                        {
                                                var ul = _map.Annotations.OfType<MKUserLocation> ().FirstOrDefault ();
                                            //  Logger.LogMessage ("GPS located ULat: " + ul.Coordinate.Latitude.ToString ());
                                             //   Logger.LogMessage ("GPS located ULon: " + ul.Coordinate.Longitude.ToString ());
                                               // Logger.LogMessage ("GPS located UAcc: " + ul.Location.HorizontalAccuracy.ToString ());
                                        }
                                        catch
                                        {
                                                
                                        }
                                        
                                        Logger.LogMessage ("GPS located CLat: " + AppContext.Current.CurrrentLocation.Coordinate.Latitude.ToString ());
                                        Logger.LogMessage ("GPS located CLong: " + AppContext.Current.CurrrentLocation.Coordinate.Longitude.ToString ());
                                        Logger.LogMessage ("GPS located CAcc: " + AppContext.Current.CurrrentLocation.HorizontalAccuracy.ToString ());
                                        
                                        
                                        
                                        LoadAddress (AppContext.Current.CurrrentLocation.Coordinate.Latitude, AppContext.Current.CurrrentLocation.Coordinate.Longitude, (count >= 39), true );
                                        
                        
				
				}
				catch (Exception ex)
				{
					Logger.LogError (ex);
				}
				finally
				{
					
					if (showProgress)
					{
						_text.InvokeOnMainThread (() => {
							LoadingOverlay.StopAnimatingLoading (_text); });
					}
				}
			});
		}

		private void LoadAddress (double latitude, double longitude, bool isGPSNotAccurate, bool setRegionOnMap)
		{
			var service = ServiceLocator.Current.GetInstance<IBookingService> ();
			var result = service.SearchAddress (latitude, longitude);
			if (  (result.Count () > 0) && ( result[0].Longitude.HasValue ) &&  ( result[0].Latitude.HasValue ) )
			{
				LocationData loc = null ;
				
				
				loc = AppContext.Current.LoggedUser.FavoriteLocations.Where (d => d.Latitude.HasValue && d.Longitude.HasValue)
					.FirstOrDefault (d => (Math.Abs (d.Longitude.Value - result [0].Longitude.Value) <= 0.002) && (Math.Abs (d.Latitude.Value - result [0].Latitude.Value) <= 0.002));
				if (loc == null)
				{
					loc = AppContext.Current.LoggedUser.BookingHistory.Where (b => !b.Hide && (b.PickupLocation !=null ) && b.PickupLocation.Latitude.HasValue && b.PickupLocation.Longitude.HasValue).Select (b => b.PickupLocation).
						FirstOrDefault (d => (Math.Abs (d.Longitude.Value - result [0].Longitude.Value) <= 0.001) && (Math.Abs (d.Latitude.Value - result [0].Latitude.Value) <= 0.001));
				}
				
				
				if (!_showCurrentLocationCanceled)
				{
					
					if (loc == null)
					{
						result [0].IsGPSDetected = true;
						result [0].IsGPSNotAccurate = isGPSNotAccurate;
						SetLocation (result [0], false, setRegionOnMap);
						
					}
					else
					{
						SetLocation (loc, true, setRegionOnMap);
					}
				}
			}
			
		}
		
		
		
	}
}

