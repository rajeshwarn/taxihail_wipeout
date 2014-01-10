using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RefineAddressView : MvxViewController
	{
		public RefineAddressView() 
			: base("RefineAddressView", null)
		{
		}

		public new RefineAddressViewModel ViewModel
		{
			get
			{
				return (RefineAddressViewModel)DataContext;
			}
		}	
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

			lblAptNumber.Text = Resources.AptNumber;
			lblRingCode.Text = Resources.RingCode;
			
			lblAptNumber.TextColor = AppStyle.TitleTextColor;
			lblRingCode.TextColor = AppStyle.TitleTextColor;
			

			txtAptNumber.TextColor = AppStyle.GreyText;
			txtRingCode.TextColor = AppStyle.GreyText;
			

			txtAptNumber.PaddingLeft = 5;
			txtRingCode.PaddingLeft = 5;
			
			txtAptNumber.ShouldReturn = ShouldReturnDelegate;
			txtRingCode.ShouldReturn = ShouldReturnDelegate;
			

			var btnDone = new UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
				if( ViewModel.SaveCommand.CanExecute() )
				{
					ViewModel.SaveCommand.Execute();
				}
			});
			NavigationItem.HidesBackButton = false;
			NavigationItem.RightBarButtonItem = btnDone;
            NavigationItem.Title = Resources.GetValue( "View_RefineAddress");

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

