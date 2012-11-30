
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Touch.Interfaces;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class TutorialView : MvxBindingTouchViewController<TutorialViewModel>, IMvxModalTouchView
    {
        #region Constructors
        
        public TutorialView () 
            : base(new MvxShowViewModelRequest<TutorialViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
            Initialize();
        }
        
        public TutorialView (MvxShowViewModelRequest request) 
            : base(request)
        {
            Initialize();
        }
        
        public TutorialView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
            Initialize();
        }
        
#endregion
        
        void Initialize()
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
            // Perform any additional setup after loading the view, typically from a nib.
            CreatePanels(ViewModel.TutorialItemsList);
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

      

        private void CreatePanels(List<TutorialItemModel> listTutorial)
        {
            scrollview.Scrolled += ScrollViewScrolled;
            
            int count = listTutorial.Count;
            RectangleF scrollFrame = scrollview.Frame;
            scrollFrame.Width = scrollFrame.Width * count;
            scrollview.ContentSize = scrollFrame.Size;

            for (int i=0; i<count; i++)
            {
                UIView view = new UIView();
                UIImageView image = new UIImageView( UIImage.FromFile("Assets/"+listTutorial[i].ImageUri+".png"));
                //image.Frame = scrollview.Frame;
                image.AutosizesSubviews =true;
                UILabel labelBottom = new UILabel();
                labelBottom.TextColor = UIColor.Black;
                labelBottom.TextAlignment = UITextAlignment.Center;
                labelBottom.Text = listTutorial[i].BottomText;
                labelBottom.Lines = 3;

                UILabel labelTop = new UILabel();
                labelTop.TextColor = UIColor.Black;
                labelTop.TextAlignment = UITextAlignment.Center;
                labelTop.Text = listTutorial[i].TopText;
                labelTop.Lines =3;


                labelTop.SetDimensions(width:scrollview.Frame.Width,height:100);
                labelTop.SetPosition(scrollview.Frame.Width*i,0);

                image.SetDimensions(width:scrollview.Frame.Width,height:200);
                image.SetPosition(scrollview.Frame.Width*i,100);

                labelBottom.SetDimensions(width:scrollview.Frame.Width,height:100);
                labelBottom.SetPosition(scrollview.Frame.Width*i,300);


                view.AddSubview(labelTop);
                view.AddSubview(image);
                view.AddSubview(labelBottom);
                scrollview.AddSubview(view);
            }
            pageControl.Hidden = false;
            pageControl.Pages = count;
        }
        private void ScrollViewScrolled (object sender, EventArgs e)
        {
            double page = Math.Floor((scrollview.ContentOffset.X - scrollview.Frame.Width / 2) / scrollview.Frame.Width) + 1;

            scrollview.ContentOffset = new PointF( scrollview.ContentOffset.X,0);

            pageControl.CurrentPage = (int)page;
        }
    }
}

