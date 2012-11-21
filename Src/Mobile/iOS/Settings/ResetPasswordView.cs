using System;
using System.Text.RegularExpressions;
using System.Linq;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.Dialog.Touch;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class ResetPasswordView : MvxTouchDialogViewController<ResetPasswordViewModel>
    {

		public ResetPasswordView(MvxShowViewModelRequest request)
			: base(request, UITableViewStyle.Grouped, null, false)
		{

		}
		
		public ResetPasswordView(MvxShowViewModelRequest request, UITableViewStyle style, RootElement root, bool pushing)
			: base(request, style, root, pushing)
		{

		}
         
		public override void ViewDidLoad()
        {
			base.ViewDidLoad();
            LoadSettingsElements();           
            
            AddButton(170, 100, Resources.ResetPasswordReset, AppStyle.AcceptButtonColor, AppStyle.AcceptButtonHighlightedColor, "ResetPassword" , apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);
			AddButton(20, 100, Resources.ResetPasswordCancel, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor, "Cancel", apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Grey);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.BackgroundColor = UIColor.Clear;
            TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear };
            TableView.BackgroundColor = UIColor.Clear;
			((UINavigationController ) ParentViewController ).NavigationBar.TopItem.TitleView = new TitleView(null, Resources.View_PasswordRecovery,true);
            ((UINavigationController ) ParentViewController ).View.BackgroundColor =UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
        }
        
        public override void LoadView()
        {
            base.LoadView();            
        }

        private void LoadSettingsElements()
        {          
			var menu = new RootElement(Resources.View_PasswordRecovery);            
			var settings = new Section(Resources.View_PasswordRecovery); 

			var _emailEntry = new RightAlignedMvvmCrossEntryElement (Resources.ResetPasswordLabel, Resources.ResetPasswordPlaceholder);
            _emailEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.EmailAddress;
            _emailEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
            _emailEntry.AutocorrectionType = UITextAutocorrectionType.No;

			_emailEntry.Bind (this, "{'Value':{'Path':'Email','Mode':'TwoWay'}}");
                       
            menu.Add(settings);
            settings.Add(_emailEntry);           
            
            this.InvokeOnMainThread(() => { this.Root = menu; });
        }

        private void LoadBackgroundNavBar(UINavigationBar bar)
        {
            bar.TintColor =  AppStyle.NavigationBarColor; 
            //It might crash on iOS version smaller than 5.0
            try
            {
                bar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
            catch{ }
        }

        private void AddButton(float x, float y, string title, UIColor normal, UIColor selected, string command, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new System.Drawing.RectangleF(x, y, 130, 40), title, bcolor);
            btn.TextShadowColor = null;
            this.View.AddSubview(btn);
			this.AddBindings(btn, "{'TouchUpInside': {'Path' : '" + command + "'}}");             
        }      
        
    }
}

