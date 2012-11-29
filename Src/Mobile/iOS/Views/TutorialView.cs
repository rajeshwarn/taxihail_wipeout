
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using MK.Common.Android.Entity;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class TutorialView : MvxBindingTouchViewController<TutorialViewModel>
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

            /*List<UIColor> listColors = new List<UIColor>(){ UIColor.Red, UIColor.Green, UIColor.Blue};

            for(int i = 0 ; i< listColors.Count; i++)
            {
                RectangleF frame;
                frame.X = this.scrollview.Frame.Size.Width * i;
                frame.Y = 0;
                frame.Size = this.scrollview.Frame.Size;
                UIView subView = new UIView(frame);
                subView.BackgroundColor = listColors[i];
                this.scrollview.AddSubview(subView);
            }
            this.scrollview.ContentSize.Width = this.scrollview.Frame.Size.Width * listColors.Count;
            this.scrollview.ContentSize.Height = this.scrollview.Frame.Size.Height;*/
            CreatePanels(ViewModel.TutorialItemsList);

            /*NSArray *colors = [NSArray arrayWithObjects:[UIColor redColor], [UIColor greenColor], [UIColor blueColor], nil];
            for (int i = 0; i < colors.count; i++) {
                CGRect frame;
                frame.origin.x = self.scrollView.frame.size.width * i;
                frame.origin.y = 0;
                frame.size = self.scrollView.frame.size;
                
                UIView *subview = [[UIView alloc] initWithFrame:frame];
                subview.backgroundColor = [colors objectAtIndex:i];
                [self.scrollView addSubview:subview];
                [subview release];
            }
            
            self.scrollView.contentSize = CGSizeMake(self.scrollView.frame.size.width * colors.count, self.scrollView.frame.size.height);*/
        

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

                UILabel labelTop = new UILabel();
                labelTop.TextColor = UIColor.Black;
                labelTop.TextAlignment = UITextAlignment.Center;
                labelTop.Text = listTutorial[i].TopText;


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
            pageControl.CurrentPage = 1;
        }
        private void ScrollViewScrolled (object sender, EventArgs e)
        {
            double page = Math.Floor((scrollview.ContentOffset.X - scrollview.Frame.Width / 2) / scrollview.Frame.Width) + 1;
            
            pageControl.CurrentPage = (int)page;
        }
    }
}

