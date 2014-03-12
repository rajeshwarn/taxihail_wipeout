using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Touch.Views;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class TutorialView : MvxViewController, IMvxModalTouchView
    {
        public TutorialView () 
			: base("TutorialView", null)
        {
            Initialize ();
        }
        
        void Initialize ()
        {
        }

		public new TutorialViewModel ViewModel
		{
			get
			{
				return (TutorialViewModel)DataContext;
			}
		}
        
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            contentView.Layer.CornerRadius = 2;
            View.BackgroundColor = UIColor.FromRGBA (0, 0, 0, 0.7f);

            CreatePanels (ViewModel.TutorialItemsList);

			var set = this.CreateBindingSet<TutorialView, TutorialViewModel>();

			set.BindSafe(btnClose)
				.For("TouchUpInside")
				.To(vm => vm.CloseCommand);

			set.Apply ();
        }

        private void CreatePanels (TutorialItemModel[] listTutorial)
        {
            scrollview.Scrolled += ScrollViewScrolled;
            var count = listTutorial.Length;
            var scrollFrame = scrollview.Frame;
            scrollFrame.Width = scrollFrame.Width * count;
            scrollview.ContentSize = scrollFrame.Size;

            for (var i = 0; i < count; i++)
            {
                var pageView = new UIView(new RectangleF(i * scrollview.Frame.Width, 0, scrollview.Frame.Width, scrollview.Frame.Height)) { BackgroundColor = UIColor.Clear };
                scrollview.AddSubview(pageView);

                var labelTopTitle = new UILabel(new RectangleF(0, 0, pageView.Frame.Width, 0))
                {
                    LineBreakMode = UILineBreakMode.WordWrap,
                    TextColor = UIColor.FromRGB(44, 44, 44),
                    BackgroundColor = UIColor.Clear,
                    TextAlignment = UITextAlignment.Center,
                    Font = UIFont.FromName(FontName.HelveticaNeueBold, 17.0f),
                    Lines = 1
                };
                labelTopTitle.Text = listTutorial[i].Title;  
                labelTopTitle.SizeToFit();
                labelTopTitle.SetX((pageView.Frame.Width - labelTopTitle.Frame.Width) / 2)
                    .SetY(0);
                pageView.AddSubview(labelTopTitle);               

                var labelTop = new UILabel(new RectangleF(40, 0, pageView.Frame.Width - 40, 0))
                {
                    LineBreakMode = UILineBreakMode.WordWrap,
                    TextColor = UIColor.FromRGB(44, 44, 44),
                    BackgroundColor = UIColor.Clear,
                    TextAlignment = UITextAlignment.Center,
                    Font = UIFont.FromName(FontName.HelveticaNeueLight, 17.0f),
                    Lines = 0
                };
                labelTop.Text = listTutorial[i].Text;  
                labelTop.SizeToFit();
                labelTop.SetX((pageView.Frame.Width - labelTop.Frame.Width) / 2)
                        .SetY(labelTopTitle.Frame.Bottom);
                pageView.AddSubview(labelTop);

                var image = new UIImageView(UIImage.FromFile(listTutorial[i].ImageUri + ".png"));
                image.SetWidth(pageView.Frame.Width);
                image.ContentMode = UIViewContentMode.ScaleAspectFit;
                image.SetX(0).SetY(96f);
                pageView.AddSubview(image);
            }
            pageControl.Hidden = false;
            pageControl.Pages = count;
        }

        private void ScrollViewScrolled (object sender, EventArgs e)
        {
            var page = Math.Floor ((scrollview.ContentOffset.X - scrollview.Frame.Width / 2) / scrollview.Frame.Width) + 1;

            scrollview.ContentOffset = new PointF (scrollview.ContentOffset.X, 0);

            pageControl.CurrentPage = (int)page;
        }
    }
}

