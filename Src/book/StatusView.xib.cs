
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using apcurium.Framework.Extensions;
using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
	public partial class StatusView : UIViewController
	{
		//private int _debugCoordinatesIndex = 0;
		public event EventHandler CloseRequested;

		private bool _closeScreenWhenCompleted;


		//private CLLocationCoordinate2D[] _debugCoordinates = new CLLocationCoordinate2D[] { new CLLocationCoordinate2D (0, 0), new CLLocationCoordinate2D (45.4993900, -73.6587000), new CLLocationCoordinate2D (45.5005400, -73.6465200), new CLLocationCoordinate2D (45.5138000, -73.6323000), new CLLocationCoordinate2D (45.5206900, -73.6280400), new CLLocationCoordinate2D (45.5201600, -73.6262500), new CLLocationCoordinate2D (45.5232500, -73.6234400) };

		private int _lastOrder;

		private AddressAnnotation _taxiPosition;
		private NSTimer _timer;
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public StatusView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public StatusView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public StatusView (BookTabView parent, BookingInfoData info, bool closeScreenWhenCompleted) : base("StatusView", null)
		{
			_closeScreenWhenCompleted = closeScreenWhenCompleted;
			BookingInfo2 = info;
			Initialize ();
		}

		void Initialize ()
		{
		}

		protected BookingInfoData BookingInfo2 { get; set; }

		public void Refresh (BookingInfoData data, bool closeScreenWhenCompleted)
		{
			_closeScreenWhenCompleted = closeScreenWhenCompleted;
			BookingInfo2 = data;
			
			if (_lastOrder != AppContext.Current.LastOrder)
			{
				LoadData (data);
			}
			
			if ( _timer == null )
			    {			
				_timer = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromSeconds (4), RefreshStatus);
				RefreshStatus ();
			    }
		}
		public override void ViewDidLoad ()
		{
			
			try
			{
				NavigationItem.HidesBackButton = true;
				
				View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
				lblStatus.Text = string.Format (Resources.StatusStatusLabel, Resources.LoadingMessage);
				
				
				btnCall.SetTitle (Resources.StatusActionButton, UIControlState.Normal);
				
				btnChangeBooking.SetTitle (Resources.ChangeBookingSettingsButton, UIControlState.Normal);
			//	btnCall.TouchUpInside += CallTouchUpInside;
				
				GlassButton.Wrap (btnCall, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor).TouchUpInside +=CallTouchUpInside;
			
				
				mapStatus.Delegate = new AddressMapDelegate ();
				
				var view = AppContext.Current.Controller.GetTitleView (null, Resources.StatusViewTitle);
				
				LoadData (BookingInfo2);
				
				this.NavigationItem.TitleView = view;
				//btnRefresh.TouchUpInside += RefreshButtonTouchUpInside;
				//GlassButton.Wrap (btnRefresh, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor).TouchUpInside +=RefreshButtonTouchUpInside;
			
			}
			catch (Exception ex)
			{
				Logger.LogError (ex);
			}
		}

		private void LoadData (BookingInfoData data)
		{
			lblTitle.Text = string.Format (Resources.StatusDescription, data.Id);
			
			
			if (mapStatus.Annotations != null)
			{
				mapStatus.RemoveAnnotations (mapStatus.Annotations.OfType<MKAnnotation> ().ToArray ());
			}
			
			var pcoordinate = data.PickupLocation.GetCoordinate ();
			mapStatus.AddAnnotation (new AddressAnnotation (pcoordinate, AddressAnnotationType.Pickup, Resources.PickupMapTitle, data.PickupLocation.Address));
			
			if (data.Status.HasValue ())
			{
				lblStatus.Text = string.Format (Resources.StatusStatusLabel, data.Status);
			}
			
			if (data.DestinationLocation.Latitude.HasValue && data.DestinationLocation.Longitude.HasValue && data.DestinationLocation.Address.HasValue ())
			{
				
				try
				{										
					var dcoordinate = data.DestinationLocation.GetCoordinate ();
					mapStatus.AddAnnotation (new AddressAnnotation (dcoordinate, AddressAnnotationType.Destination, Resources.DestinationMapTitle, data.DestinationLocation.Address));
					
					double latDelta = Math.Abs (dcoordinate.Latitude - pcoordinate.Latitude);
					double longDelta = Math.Abs (dcoordinate.Longitude - pcoordinate.Longitude);
					
					var center = new CLLocationCoordinate2D ((pcoordinate.Latitude + dcoordinate.Latitude) / 2, (pcoordinate.Longitude + dcoordinate.Longitude) / 2);
					
					mapStatus.SetRegion (new MKCoordinateRegion (center, new MKCoordinateSpan (latDelta, longDelta)), true);
					
				}
				catch (Exception ex)
				{
					Console.Write (ex.Message);
				}
			}

			
			else
			{
				mapStatus.SetRegion (new MKCoordinateRegion (pcoordinate, new MKCoordinateSpan (0.02, 0.02)), true);
				
			}
			
			if (_timer != null)
			{
				_timer.Dispose ();
				_timer = null;
			}
			
			_timer = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromSeconds (4), RefreshStatus);
			RefreshStatus ();
			
		}
		private void RefreshStatus ()
		{
			ThreadHelper.ExecuteInThread (() =>
			{
				
				
				try
				{
					if (_closeScreenWhenCompleted)
					{
						var isCompleted = ServiceLocator.Current.GetInstance<IBookingService> ().IsCompleted (AppContext.Current.LoggedUser, BookingInfo2.Id);
						
						//|| (_debugCoordinatesIndex == 6))
						if (isCompleted)
						{
							//_debugCoordinatesIndex = 0;
							_timer.Dispose ();
							_timer = null;
							if (CloseRequested != null)
							{
								InvokeOnMainThread (() => CloseRequested (this, EventArgs.Empty));
							}
							return;
						}
						
					}
					
					Console.WriteLine ("Refreshing timer");
					var status = ServiceLocator.Current.GetInstance<IBookingService> ().GetOrderStatus (AppContext.Current.LoggedUser, BookingInfo2.Id);
					
					_lastOrder = BookingInfo2.Id;
					
					
					Console.WriteLine ("Refreshing timer - S: " + status.Status);
					Console.WriteLine ("Refreshing timer -LA: " + status.Latitude.ToString ());
					Console.WriteLine ("Refreshing timer -LO: " + status.Longitude.ToString ());
					
					Console.WriteLine ("Refreshing timer -Id: " + BookingInfo2.Id.ToString ());
					Console.WriteLine ("Refreshing timer -Adrs: " + BookingInfo2.PickupLocation.Address.ToString ());
					Console.WriteLine ("Refreshing timer -Adrs LO: " + BookingInfo2.PickupLocation.Longitude.ToString ());
					Console.WriteLine ("Refreshing timer -Adrs LA: " + BookingInfo2.PickupLocation.Latitude.ToString ());
					
					if (status != null)
					{
						BookingInfo2.Status = status.Status;
						InvokeOnMainThread (() => lblStatus.Text = string.Format (Resources.StatusStatusLabel, status.Status));
						
						CLLocationCoordinate2D position = new CLLocationCoordinate2D (status.Latitude, status.Longitude);
						
//					if (_debugCoordinatesIndex < _debugCoordinates.Count ())
//					{
//						position = new CLLocationCoordinate2D (_debugCoordinates[_debugCoordinatesIndex].Latitude, _debugCoordinates[_debugCoordinatesIndex].Longitude);
//					}
						
						if ((position.Latitude != 0) && (position.Longitude != 0))
						{
							
							if (_taxiPosition != null)
							{
								InvokeOnMainThread (() => { mapStatus.RemoveAnnotation (_taxiPosition); });
							}
							
							Console.WriteLine ("Show taxi 1 :" + position.Latitude.ToString ());
							
							_taxiPosition = new AddressAnnotation (position, AddressAnnotationType.Taxi, Resources.TaxiMapTitle, "");
							
							InvokeOnMainThread (() =>
							{
								mapStatus.AddAnnotation (_taxiPosition);
								mapStatus.SetCenterCoordinate (position, true);
								mapStatus.SetRegion (new MKCoordinateRegion (position, new MKCoordinateSpan (0.01, 0.01)), true);
								
							});
							
							
						}
						//_debugCoordinatesIndex++;
						
					}
					
				}
				catch (Exception ex)
				{
					Logger.LogError (ex);
				}
			});
		}

		void RefreshButtonTouchUpInside (object sender, EventArgs e)
		{
			
		}

		public override void ViewDidDisappear (bool animated)
		{
			try
			{
				base.ViewDidDisappear (animated);
				_timer.Dispose ();
				_timer = null;
			}
			catch (Exception ex)
			{
				Logger.LogError (ex);
			}
		}
		void CallTouchUpInside (object sender, EventArgs e)
		{
			
			var actionSheet = new UIActionSheet ("");
			actionSheet.AddButton (string.Format (Resources.CallCompanyButton, BookingInfo2.Settings.CompanyName));
			actionSheet.AddButton (Resources.StatusActionBookButton);
			actionSheet.AddButton (Resources.StatusActionCancelButton);
			actionSheet.AddButton (Resources.CancelBoutton);
			actionSheet.CancelButtonIndex = 3;
			actionSheet.DestructiveButtonIndex = 2;
			actionSheet.Clicked += delegate(object se, UIButtonEventArgs ea) {
				
				if (ea.ButtonIndex == 0)
				{
					var call = new Confirmation ();
					call.Call (AppSettings.PhoneNumber (AppContext.Current.LoggedUser.DefaultSettings.Company), AppSettings.PhoneNumberDisplay (AppContext.Current.LoggedUser.DefaultSettings.Company));
				}



				else if (ea.ButtonIndex == 1)
				{
					var newBooking = new Confirmation ();
					newBooking.Action (Resources.StatusConfirmNewBooking, () =>
					{
						_timer.Dispose ();
						_timer = null;
						if (CloseRequested != null)
						{
							InvokeOnMainThread (() => CloseRequested (this, EventArgs.Empty));
						}
					});
					
				}
				else if (ea.ButtonIndex == 2)
				{
					var newBooking = new Confirmation ();
					newBooking.Action (Resources.StatusConfirmCancelRide, () =>
					{
						
						LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
						View.UserInteractionEnabled = false;
						
						ThreadHelper.ExecuteInThread (() =>
						{
							try
							{
								var isSuccess = ServiceLocator.Current.GetInstance<IBookingService> ().CancelOrder (AppContext.Current.LoggedUser, BookingInfo2.Id);
								
								if (isSuccess)
								{
									_timer.Dispose ();
									
									_timer = null;
									if (CloseRequested != null)
									{
										InvokeOnMainThread (() => CloseRequested (this, EventArgs.Empty));
									}
								}

								
								else
								{
									
									MessageHelper.Show (Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
								}
							}
							finally
							{
								InvokeOnMainThread (() =>
								{
									LoadingOverlay.StopAnimatingLoading (this.View);
									View.UserInteractionEnabled = true;
								});
							}
						});
					});
					//		
				}
				
				
			};
			actionSheet.ShowInView (AppContext.Current.Controller.View);
			
			
			
			
		}
		
		
		#endregion
	}
}

