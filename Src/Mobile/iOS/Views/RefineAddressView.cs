using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class RefineAddressView : MvxBindingTouchViewController<RefineAddressViewModel>
	{
		#region Constructors
		public RefineAddressView(string apt, string ringCode, string buildingName) 
			: base(new MvxShowViewModelRequest<RefineAddressViewModel>( new Dictionary<string, string>(){{"apt", apt}, {"ringCode", ringCode},  {"buildingName", buildingName}}, false, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
		}
		
		public RefineAddressView(MvxShowViewModelRequest request) 
			: base(request)
		{
		}
		
		public RefineAddressView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
		}	
		#endregion
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

			lblAptNumber.Text = Resources.AptNumber;
			lblRingCode.Text = Resources.RingCode;
			lblBuildingName.Text = Resources.BuildingName;
			lblAptNumber.TextColor = AppStyle.TitleTextColor;
			lblRingCode.TextColor = AppStyle.TitleTextColor;
			lblBuildingName.TextColor = AppStyle.TitleTextColor;

			txtAptNumber.TextColor = AppStyle.GreyText;
			txtRingCode.TextColor = AppStyle.GreyText;
			txtBuildingName.TextColor = AppStyle.GreyText;

			txtAptNumber.PaddingLeft = 5;
			txtRingCode.PaddingLeft = 5;
			txtBuildingName.PaddingLeft = 5;

			txtAptNumber.ShouldReturn = ShouldReturnDelegate;
			txtRingCode.ShouldReturn = ShouldReturnDelegate;
			txtBuildingName.ShouldReturn = ShouldReturnDelegate;

			var btnDone = new UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
				if( ViewModel.SaveCommand.CanExecute() )
				{
					ViewModel.SaveCommand.Execute();
				}
			});
			NavigationItem.HidesBackButton = true;
			NavigationItem.RightBarButtonItem = btnDone;
            NavigationItem.Title = Resources.GetValue( "View_RefineAddress");

			this.AddBindings(new Dictionary<object, string>(){
				{txtAptNumber, "{'Text':{'Path':'AptNumber'}}"} ,
				{txtRingCode, "{'Text':{'Path':'RingCode'}}"} ,
				{txtBuildingName, "{'Text':{'Path':'BuildingName'}}"} ,
			});
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

		private bool ShouldReturnDelegate( UITextField textField )
		{
			return textField.ResignFirstResponder();
		}
	}
}

