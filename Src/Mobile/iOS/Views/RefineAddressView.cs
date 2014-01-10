using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RefineAddressView : MvxBindingTouchViewController<RefineAddressViewModel>
	{
		#region Constructors
		public RefineAddressView(string apt, string ringCode, string buildingName) 
			: base(new MvxShowViewModelRequest<RefineAddressViewModel>( new Dictionary<string, string>{{"apt", apt}, {"ringCode", ringCode},  {"buildingName", buildingName}}, false, new MvxRequestedBy()   ) )
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
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            lblAptNumber.Text = Localize.GetValue("AptNumber");
            lblRingCode.Text = Localize.GetValue("RingCode");
			
			lblAptNumber.TextColor = AppStyle.TitleTextColor;
			lblRingCode.TextColor = AppStyle.TitleTextColor;
			

			txtAptNumber.TextColor = AppStyle.GreyText;
			txtRingCode.TextColor = AppStyle.GreyText;
			

			txtAptNumber.PaddingLeft = 5;
			txtRingCode.PaddingLeft = 5;
			
			txtAptNumber.ShouldReturn = ShouldReturnDelegate;
			txtRingCode.ShouldReturn = ShouldReturnDelegate;


            var btnDone = new UIBarButtonItem(Localize.GetValue("DoneButton"), UIBarButtonItemStyle.Plain, delegate
            {
				if( ViewModel.SaveCommand.CanExecute() )
				{
					ViewModel.SaveCommand.Execute();
				}
			});
			NavigationItem.HidesBackButton = false;
			NavigationItem.RightBarButtonItem = btnDone;
            NavigationItem.Title = Localize.GetValue( "View_RefineAddress");

			this.AddBindings(new Dictionary<object, string>{
				{txtAptNumber, "{'Text':{'Path':'AptNumber'}}"} ,
				{txtRingCode, "{'Text':{'Path':'RingCode'}}"} ,			
			});
            View.ApplyAppFont ();
		}
		
		
		private bool ShouldReturnDelegate( UITextField textField )
		{
			return textField.ResignFirstResponder();
		}
	}
}

