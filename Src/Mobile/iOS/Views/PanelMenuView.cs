
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using apcurium.MK.Booking.Mobile.Client.Animations;
using MonoTouch.MessageUI;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.IO;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using Cirrious.MvvmCross.Touch.Interfaces;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class PanelMenuView : MvxBindingTouchViewController<PanelViewModel>
	{
		private UINavigationController _navController;
		private UIView _viewToAnimate;
		private bool _menuIsOpen = false;

		public PanelMenuView(UIView viewToAnimate, UINavigationController navController) 
			: base(new MvxShowViewModelRequest<PanelViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
			_navController = navController;
			_viewToAnimate = viewToAnimate;
		}
		
		public PanelMenuView(MvxShowViewModelRequest request) 
			: base(request)
		{

		}
		
		public PanelMenuView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{

		}

		
		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.Frame = new RectangleF( View.Frame.X, View.Frame.Y, View.Frame.Width, View.Frame.Height );
			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
			
			panelView.Title = Resources.TabSettings;
			
			logoImageView.Image = UIImage.FromFile( "Assets/apcuriumLogo.png" );
			versionLabel.Text = TinyIoCContainer.Current.Resolve<IPackageInfo>().Version;

			InitializeMenu();	

		}

		private void InitializeMenu()
		{
			var structure = new InfoStructure( 44, false );
			var sect = structure.AddSection();
            sect.AddItem( new SingleLineItem( Resources.GetValue("View_Book_Menu_MyLocations")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
					AnimateMenu();
					_navController.PushViewController(new LocationsTabView(), true);
				})				
			});
            sect.AddItem( new SingleLineItem( Resources.GetValue("View_Book_Menu_MyOrders") ) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
					AnimateMenu();
					_navController.PushViewController(new HistoryTabView(), true);
				})				
			});
            sect.AddItem( new SingleLineItem( Resources.GetValue("View_Book_Menu_UpdateMyProfile")   ) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
					AnimateMenu();
					var rideSettingsView = new RideSettingsView (AppContext.Current.LoggedUser, true, false);
					_navController.PushViewController( rideSettingsView, true);
				})				
			});
            sect.AddItem( new SingleLineItem( Resources.GetValue("View_Book_Menu_CallDispatch")   ) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
					AnimateMenu();
					var call = new Confirmation ();
					call.Call ( TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumber(AppContext.Current.LoggedUser.Settings.ProviderId.Value),
					           TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumberDisplay (AppContext.Current.LoggedUser.Settings.ProviderId.Value));
				})				
			});
            sect.AddItem( new SingleLineItem( Resources.GetValue("View_Book_Menu_AboutUs") ) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
					AnimateMenu();
					_navController.PushViewController(new AboutUsView(), true);
				})				
			});
            sect.AddItem( new SingleLineItem( Resources.GetValue("View_Book_Menu_ReportProblem") ) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
					AnimateMenu();
					if (!MFMailComposeViewController.CanSendMail)
					{
						return;
					}
					
					var mailComposer = new MFMailComposeViewController ();
					
					if (File.Exists (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog))
					{
						mailComposer.AddAttachmentData (NSData.FromFile (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog), "text", "errorlog.txt");
					}
					
					mailComposer.SetToRecipients (new string[] { TinyIoCContainer.Current.Resolve<IAppSettings>().SupportEmail  });
					mailComposer.SetMessageBody ("", false);
					mailComposer.SetSubject (Resources.TechSupportEmailTitle);
					mailComposer.Finished += delegate(object mailsender, MFComposeResultEventArgs mfce) {
						mailComposer.DismissModalViewControllerAnimated (true);
						if (File.Exists (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog))
						{
							File.Delete (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog);
						}
					};
					_navController.PresentModalViewController(mailComposer, true);
				})				
			});
			sect.AddItem( new SingleLineItem( Resources.View_Book_Menu_SignOut ) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
					AnimateMenu();
					AppContext.Current.WarnEstimate = true;
					ViewModel.SignOut();

				})				
			});

            menuListView.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            menuListView.BackgroundColor = UIColor.Clear; // UIColor.Red ;
            menuListView.ScrollEnabled = false;
			menuListView.DataSource = new TableViewDataSource( structure );
			menuListView.Delegate = new TableViewDelegate( structure );
			menuListView.ReloadData();
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		public void AnimateMenu()
		{
			var slideAnimation = new SlideViewAnimation( _viewToAnimate, new SizeF( (_menuIsOpen ? menuView.Frame.Width : -menuView.Frame.Width), 0f ) );
			slideAnimation.Animate();
			_menuIsOpen = !_menuIsOpen;
		}


	}
}

