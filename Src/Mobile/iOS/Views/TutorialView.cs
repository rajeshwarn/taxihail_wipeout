
using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Dialog.Touch.Dialog;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Touch.Interfaces;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class TutorialView : MvxBindingTouchViewController<TutorialViewModel>, IMvxModalTouchView
    {
        #region Constructors
        
        public TutorialView () 
            : base(new MvxShowViewModelRequest<TutorialViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
            Initialize ();
        }
        
        public TutorialView (MvxShowViewModelRequest request) 
            : base(request)
        {
            Initialize ();
        }
        
        public TutorialView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
            Initialize ();
        }
        
#endregion
        
        void Initialize ()
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

            contentView.Layer.CornerRadius = 7;
            btnClose.SetImage (UIImage.FromFile ("Assets/closeButton.png"), UIControlState.Normal);
            btnClose.SetTitle ("", UIControlState.Normal);
            View.BackgroundColor = UIColor.FromRGBA (0, 0, 0, 0.40f);
            this.AddBindings (new Dictionary<object, string> (){ 
                { btnClose, "{'TouchUpInside':{'Path':'CloseCommand'}}"} 
            });
            
            CreatePanels (ViewModel.TutorialItemsList);

            //AppStyle.ApplyAppFont ();

        }

        private void CreatePanels (TutorialItemModel[] listTutorial)
        {
            scrollview.Scrolled += ScrollViewScrolled;
            int count = listTutorial.Length;
            RectangleF scrollFrame = scrollview.Frame;
            scrollFrame.Width = scrollFrame.Width * count;
            scrollview.ContentSize = scrollFrame.Size;

            for (int i=0; i<count; i++) {
                var view = new UIView ();
                view.BackgroundColor = UIColor.Clear;
                var image = new UIImageView (UIImage.FromFile ("Assets/Tutorial/" + listTutorial [i].ImageUri + ".png"));

                var labelBottom = new ValignLabel ();
                labelBottom.VerticalAlignment = ValignLabel.VerticalAlignments.Middle;
                labelBottom.TextColor = AppStyle.GreyText;
                labelBottom.BackgroundColor = UIColor.Clear;
                labelBottom.TextAlignment = UITextAlignment.Center;
                labelBottom.Text = listTutorial [i].BottomText;
                labelBottom.Font = AppStyle.GetNormalFont ( 16 );
                labelBottom.Lines = 0;
                labelBottom.Frame = new RectangleF( scrollview.Frame.Width * i, scrollview.Frame.Height - 110, scrollview.Frame.Width, 95 ); ;
                view.AddSubview (labelBottom);


                var labelBottomTitle = new ValignLabel ();
                labelBottomTitle.VerticalAlignment = ValignLabel.VerticalAlignments.Middle;
                labelBottomTitle.TextColor = AppStyle.GreyText;
                labelBottomTitle.BackgroundColor = UIColor.Clear;
                labelBottomTitle.TextAlignment = UITextAlignment.Center;
                labelBottomTitle.Text = listTutorial [i].BottomTitle;
                labelBottomTitle.Font = AppStyle.GetBoldFont ( 18 );
                labelBottomTitle.Lines = 1;
                labelBottomTitle.Frame = new RectangleF( scrollview.Frame.Width * i, scrollview.Frame.Height - 40, scrollview.Frame.Width, 30 ); 
                view.AddSubview (labelBottomTitle);



                var labelTop = new ValignLabel ();
                labelTop.VerticalAlignment = ValignLabel.VerticalAlignments.Middle ;
                labelTop.BackgroundColor = UIColor.Clear;
                labelTop.TextColor = AppStyle.DarkText;
                labelTop.TextAlignment = UITextAlignment.Center;
                labelTop.Text = listTutorial [i].TopText;
                labelTop.Font = AppStyle.GetNormalFont ( 16 );
                labelTop.Lines = 0;
                labelTop.Frame = new RectangleF (scrollview.Frame.Width * i, 10, scrollview.Frame.Width, 110);                
                view.AddSubview (labelTop);

                var labelTopTitle = new ValignLabel ();
                labelTopTitle.VerticalAlignment = ValignLabel.VerticalAlignments.Middle;
                labelTopTitle.TextColor =  AppStyle.DarkText;
                labelTopTitle.BackgroundColor = UIColor.Clear;
                labelTopTitle.TextAlignment = UITextAlignment.Center;
                labelTopTitle.Text = listTutorial [i].TopTitle;
                labelTopTitle.Font = AppStyle.GetBoldFont ( 18 );
                labelTopTitle.Lines = 1;
                labelTopTitle.Frame = new RectangleF (scrollview.Frame.Width * i, 20, scrollview.Frame.Width, 30);                
                view.AddSubview (labelTopTitle);

                image.SetPosition (scrollview.Frame.Width * i, 115);





                view.AddSubview (image);

                scrollview.AddSubview (view);
            }
            pageControl.Hidden = false;
            pageControl.Pages = count;
        }

        private void ScrollViewScrolled (object sender, EventArgs e)
        {
            double page = Math.Floor ((scrollview.ContentOffset.X - scrollview.Frame.Width / 2) / scrollview.Frame.Width) + 1;

            scrollview.ContentOffset = new PointF (scrollview.ContentOffset.X, 0);

            pageControl.CurrentPage = (int)page;
        }
    }
}

