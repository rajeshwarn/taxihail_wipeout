using System;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using TaxiMobile.Helper;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.Localization;

namespace TaxiMobile.Book
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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			imageFieldBackground.Image = UIImage.FromFile ("Assets/TopFieldBackground-Sm.png");
			
			mapDestination.Delegate = new AddressMapDelegate ();
			
			_addressController = new AddressContoller (txtAddress, null, null, tableSimilarAddress, btnPickAddress, mapDestination, AddressAnnotationType.Destination, Resources.DestinationMapTitle,
			                                           () => _parent.BookingInfo.DestinationLocation, data => _parent.BookingInfo.DestinationLocation = data, () => _parent.IsTopView);
			
			_addressController.LocationHasChanged += Handle_addressControllerLocationHasChanged;
			lblDestination.Text = Resources.DestinationViewDestinationLabel;
			
			DisplayEstimate ();
			
			txtAddress.Placeholder = Resources.DestinationTextPlaceholder;
			
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
				var distance = _parent.BookingInfo.GetDistance ();
				var price = _parent.BookingInfo.GetPrice (distance);
				InvokeOnMainThread (() =>
				{
					
					
					if (distance.HasValue)
					{						
						lblDistance.Text = string.Format (Resources.EstimateDistance, Math.Round (distance.Value / 1000, 1).ToString () + "km");
					}
					else
					{
						lblDistance.Text = string.Format (Resources.EstimateDistance, Resources.NotAvailable);
					}
					
					lblPrice.TextColor = UIColor.Black;
					if (price.HasValue)
					{
						if (price.Value > 100)
						{
							lblPrice.Text = Resources.EstimatePriceOver100;
							lblPrice.TextColor = UIColor.Red;
						}
						else
						{
							lblPrice.Text = string.Format (Resources.EstimatePrice, string.Format ("{0:c}", price.Value));							
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
			var service = ServiceLocator.Current.GetInstance<IBookingService> ();
			var result = service.SearchAddress (txtAddress.Text);
			
			if (result.Any())
			{
				SetDestinationLocation (result [0]);
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

		private void SetDestinationLocation (LocationData locationData)
		{
			BookingInfoData data = _parent.BookingInfo;
			
			data.DestinationLocation = locationData;
			
			InvokeOnMainThread (() => txtAddress.Text = locationData.Address);
			
			if ((mapDestination.Annotations != null) && (mapDestination.Annotations.OfType<AddressAnnotation> ().Count (a => a.AddressType == AddressAnnotationType.Destination) > 0))
			{
				InvokeOnMainThread (() => mapDestination.RemoveAnnotations (mapDestination.Annotations.OfType<AddressAnnotation> ().Where (a => a.AddressType == AddressAnnotationType.Destination).ToArray ()));
			}
			
			InvokeOnMainThread (() =>
			{
				_addressController.SuspendRegionChanged ();
				
				try
				{
					var coordinate = data.DestinationLocation.GetCoordinate ();
					mapDestination.SetRegion (new MKCoordinateRegion (coordinate, new MKCoordinateSpan (0.02, 0.02)), true);
					mapDestination.AddAnnotation (new AddressAnnotation (coordinate, AddressAnnotationType.Destination, Resources.DestinationMapTitle, data.DestinationLocation.Address));
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

