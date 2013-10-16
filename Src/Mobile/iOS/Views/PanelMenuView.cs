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
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class PanelMenuView : UIViewController
    {
        private UINavigationController _navController;
        private UIView _viewToAnimate;
        private PanelViewModel _viewModel;

        public PanelMenuView (IntPtr handle) : base(handle)
        {
        }
        
        [Export("initWithCoder:")]
        public PanelMenuView (NSCoder coder) : base(coder)
        {
        }
        
        public PanelMenuView () : base("PanelMenuView", null)
        {
        }

        public PanelMenuView (UIView viewToAnimate, UINavigationController navController, PanelViewModel viewModel) 
            : this()
        {
            _navController = navController;
            _viewToAnimate = viewToAnimate;
            _viewModel = viewModel;
        }
        
        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.Frame = new RectangleF (View.Frame.X, View.Frame.Y, View.Frame.Width, View.Frame.Height);
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
            
            panelView.Title = Resources.TabSettings;
            
            logoImageView.Image = UIImage.FromFile ("Assets/apcuriumLogo.png");
            versionLabel.Text = TinyIoCContainer.Current.Resolve<IPackageInfo> ().Version;

            InitializeMenu ();  
            this.View.ApplyAppFont ();
        }

        private void InitializeMenu ()
        {
          
            var structure = new InfoStructure (40, false);
            var sect = structure.AddSection ();
            sect.AddItem (new SingleLineItem (Resources.GetValue ("View_Book_Menu_MyLocations")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                    _viewModel.NavigateToMyLocations.Execute();
                })              
            });
            sect.AddItem (new SingleLineItem (Resources.GetValue ("View_Book_Menu_MyOrders")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                    _viewModel.NavigateToOrderHistory.Execute();
                })              
            });
            sect.AddItem (new SingleLineItem (Resources.GetValue ("View_Book_Menu_UpdateMyProfile")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                    _viewModel.NavigateToUpdateProfile.Execute();
                })              
            });

            if (_viewModel.TutorialEnabled) {
                sect.AddItem (new SingleLineItem (Resources.GetValue ("View_Book_Menu_Tutorial")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                    _viewModel.NavigateToTutorial.Execute();
                })              
            });
            }

            if (_viewModel.CanCall) {               
                sect.AddItem (new SingleLineItem (Resources.GetValue ("View_Book_Menu_CallDispatch")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                    _viewModel.Call.Execute();
                })              
            });
            }
            sect.AddItem (new SingleLineItem (Resources.GetValue ("View_Book_Menu_AboutUs")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                    _viewModel.NavigateToAboutUs.Execute();
                })              
            });

            if (_viewModel.CanReportProblem) {
                sect.AddItem (new SingleLineItem (Resources.GetValue ("View_Book_Menu_ReportProblem")) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                        _viewModel.ReportProblem.Execute();                 
                    })              
                });
            }
            sect.AddItem (new SingleLineItem (Resources.View_Book_Menu_SignOut) { OnItemSelected = sectItem => InvokeOnMainThread(() => { 
                    _viewModel.SignOut.Execute();
                })              
            });

            menuListView.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            menuListView.BackgroundColor = UIColor.Clear;
            menuListView.ScrollEnabled = false;
            menuListView.DataSource = new TableViewDataSource (structure);
            menuListView.Delegate = new TableViewDelegate (structure);
            menuListView.ReloadData ();

            _viewModel.PropertyChanged += HandlePropertyChanged;
        }

        void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "MenuIsOpen") {
                AnimateMenu ();
            }
        }

        private void AnimateMenu ()
        {
            InvokeOnMainThread (() =>
            {
                var slideAnimation = new SlideViewAnimation (_viewToAnimate, new SizeF ((_viewModel.MenuIsOpen ? -menuView.Frame.Width : menuView.Frame.Width), 0f));
                slideAnimation.Animate ();
            });
        }
//
//        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
//        {
//            return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
//        }
//
//        public override void ViewDidUnload ()
//        {
//            base.ViewDidUnload ();
//            
//            ReleaseDesignerOutlets ();
//        }
//
//        public override void ViewWillUnload ()
//        {
//            base.ViewWillUnload ();
//            _viewModel.PropertyChanged -= HandlePropertyChanged;
//        }
            

    }
}


