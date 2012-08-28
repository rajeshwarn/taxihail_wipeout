using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.Framework.Extensions;


namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class DestinationView : UIViewController
	{
		private BookTabView _parent;
		private AddressContoller _addressController;

		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public DestinationView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public DestinationView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public DestinationView (BookTabView parent) : base("DestinationView", null)
		{
			_parent = parent;
			Initialize ();
		}

		void Initialize ()
		{
		}

		public void StartTrackingMapMoving()
		{
			_addressController.StartTrackingMapMoving ();
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();			

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			imageFieldBackground.Image = UIImage.FromFile ("Assets/TopFieldBackground-Sm.png");
			
			mapDestination.Delegate = new AddressMapDelegate ();

					
			_addressController = new AddressContoller (txtAddress, null, null, tableSimilarAddress, mapDestination, AddressAnnotationType.Destination, Resources.DestinationMapTitle,
			                                           () => _parent.BookingInfo.DropOffAddress, data => _parent.BookingInfo.DropOffAddress = data, () => _parent.IsTopView);
			
			_addressController.CenterMapOnUserLocation();

			_addressController.LocationHasChanged += Handle_addressControllerLocationHasChanged;
			lblDestination.Text = Resources.DestinationViewDestinationLabel;
			
			DisplayEstimate ();
			
			txtAddress.Placeholder = Resources.DestinationTextPlaceholder;

            ((TextField)txtAddress).PaddingLeft = 3;
			
			mapDestination.ShowsUserLocation = true;
		}

		public void Display ()
		{
			DisplayEstimate ();
		}

		void Handle_addressControllerLocationHasChanged (object sender, EventArgs e)
		{
			DisplayEstimate ();
		}

		private void DisplayEstimate ()
		{
			ThreadHelper.ExecuteInThread (() =>
			{
                var directionInfo  = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo( _parent.BookingInfo.PickupAddress, _parent.BookingInfo.DropOffAddress );
				
				InvokeOnMainThread (() =>
				{
					
					
					if (directionInfo.Distance.HasValue)
					{						
						lblDistance.Text = string.Format (Resources.EstimateDistance, directionInfo.FormattedDistance);
					}
					else
					{
						lblDistance.Text = string.Format (Resources.EstimateDistance, Resources.NotAvailable);
					}
					
					lblPrice.TextColor = UIColor.Black;
					if (directionInfo.Price.HasValue)
					{
						if (directionInfo.Price.Value > 100)
						{
							lblPrice.Text = Resources.EstimatePriceOver100;
							lblPrice.TextColor = UIColor.Red;
						}
						else
						{
							lblPrice.Text = string.Format (Resources.EstimatePrice, directionInfo.FormattedPrice);							
						}
					}
					else
					{
						lblPrice.Text = string.Format (Resources.EstimatePrice, Resources.NotAvailable);
					}
				});
			});
			
		}

		void SearchAddress (object sender, EventArgs e)
		{
			var service = TinyIoCContainer.Current.Resolve<IGeolocService> ();
			var result = service.ValidateAddress (txtAddress.Text);
			
			if (result != null )
			{
				SetDestinationLocation (result);
			}
			
			
		}

		public void PrepareData ()
		{
			_addressController.PerpareData ();
			
			
		}
		
		public void SuspendRegionChanged ()
		{
			_addressController.SuspendRegionChanged ();
		}

		public void ResumeRegionChanged ()
		{
			_addressController.ResumeRegionChanged ();
		}

		public void AssignData ()
		{
			_addressController.AssignData ();
		}

		public void RefreshData ()
		{
			
		}

		private void SetDestinationLocation (Address locationData)
		{
			var data = _parent.BookingInfo;
			
			data.DropOffAddress = locationData;
			
			InvokeOnMainThread (() => txtAddress.Text = locationData.FullAddress);
			
			if ((mapDestination.Annotations != null) && (mapDestination.Annotations.OfType<AddressAnnotation> ().Count (a => a.AddressType == AddressAnnotationType.Destination) > 0))
			{
				InvokeOnMainThread (() => mapDestination.RemoveAnnotations (mapDestination.Annotations.OfType<AddressAnnotation> ().Where (a => a.AddressType == AddressAnnotationType.Destination).ToArray ()));
			}
			
			InvokeOnMainThread (() =>
			{
				_addressController.SuspendRegionChanged ();
				
				try
				{
					var coordinate = data.DropOffAddress.GetCoordinate ();
					mapDestination.SetRegion (new MKCoordinateRegion (coordinate, new MKCoordinateSpan (0.02, 0.02)), true);
					mapDestination.AddAnnotation (new AddressAnnotation (coordinate, AddressAnnotationType.Destination, Resources.DestinationMapTitle, data.DropOffAddress.FullAddress));
				}
				finally
				{
					_addressController.ResumeRegionChanged ();
				}
			});
			
			
		}


		
		#endregion
	}
}

