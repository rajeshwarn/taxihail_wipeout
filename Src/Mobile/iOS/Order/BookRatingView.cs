
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class BookRatingView : MvxBindingTouchViewController<BookRatingViewModel>
	{
		public BookRatingView() 
			: base(new MvxShowViewModelRequest<BookViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
		}
		
		public BookRatingView(MvxShowViewModelRequest request) 
			: base(request)
		{
		}
		
		public BookRatingView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
			submitRatingBtn.SetTitle(Resources.Submit, UIControlState.Normal);

			var source = new MvxActionBasedBindableTableViewSource(
				ratingTableView,
				UITableViewCellStyle.Default,
				BookRatingCell.Identifier ,
				BookRatingCell.BindingText,
				UITableViewCellAccessory.None);
			
			source.CellCreator = (tableView, indexPath, item) =>
			{
				return BookRatingCell.LoadFromNib(tableView);
			};
					
			this.AddBindings(new Dictionary<object, string>()                            {
				{ submitRatingBtn, "{'TouchUpInside':{'Path':'RateOrder'}, 'Hidden':{'Path': 'CanRating', 'Converter':'BoolInverter'}, 'Enabled': {'Path': 'CanSubmit'}}"},                
				{ source, "{'ItemsSource':{'Path':'RatingList'}}" }
			});

            ratingTableView.BackgroundColor = UIColor.Clear;
            ratingTableView.BackgroundView = new UIView();
			ratingTableView.Source = source;
			ratingTableView.ReloadData();

		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}


	}
}

