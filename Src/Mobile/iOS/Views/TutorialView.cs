using System;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Touch.Views;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class TutorialView : MvxViewController, IMvxModalTouchView
    {
        static readonly nfloat pageYOffsetForSmallScreen = 3, minimalScreenHeight = 480;

        private int PageCount;

        public TutorialView () : base("TutorialView", null)
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

            if (UIScreen.MainScreen.Bounds.Height <= minimalScreenHeight)
            {
                nfloat previousYScrollView = scrollview.Frame.Y;
                scrollview.SetHeight(scrollview.Frame.Height + scrollview.Frame.Y);
                scrollview.SetY(pageYOffsetForSmallScreen);

                for (int i = 0; i < contentView.Constraints.Length; i++)
                {
                    if (contentView.Constraints[i].FirstItem == scrollview && contentView.Constraints[i].FirstAttribute == NSLayoutAttribute.Top)
                    {
                        contentView.Constraints[i].Constant = pageYOffsetForSmallScreen;
                    }
                }
            }

            PageCount = listTutorial.Length;

            for (var i = 0; i < PageCount; i++)
            {
                var pageView = new UIView { BackgroundColor = UIColor.Clear };
                pageView.TranslatesAutoresizingMaskIntoConstraints = false;
                scrollview.AddSubview(pageView);

                var labelTopTitle = new UILabel
                {
                    LineBreakMode = UILineBreakMode.WordWrap,
                    TextColor = UIColor.FromRGB(44, 44, 44),
                    BackgroundColor = UIColor.Clear,
                    TextAlignment = UITextAlignment.Center,
                    Font = UIFont.FromName(FontName.HelveticaNeueBold, 17.0f),
                    Lines = 1,
                    Text = listTutorial[i].Title
                };
                labelTopTitle.TranslatesAutoresizingMaskIntoConstraints = false;

                var labelTop = new UILabel(new CGRect(40, 0, pageView.Frame.Width - 40, 0))
                {
                    LineBreakMode = UILineBreakMode.WordWrap,
                    TextColor = UIColor.FromRGB(44, 44, 44),
                    BackgroundColor = UIColor.Clear,
                    TextAlignment = UITextAlignment.Center,
                    Font = UIFont.FromName(FontName.HelveticaNeueLight, 17.0f),
                    Lines = 0,
                    Text = listTutorial[i].Text
                };
                labelTop.TranslatesAutoresizingMaskIntoConstraints = false;

                var image = new UIImageView
                {
                    Image = UIImage.FromBundle (listTutorial [i].ImageUri),
                    ContentMode = UIViewContentMode.ScaleAspectFit
                };
                image.TranslatesAutoresizingMaskIntoConstraints = false;

                pageView.AddSubviews(labelTopTitle, labelTop, image);

                // constraints for pageView
                View.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(pageView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, scrollview, NSLayoutAttribute.Height, 1f, 0f),
                    NSLayoutConstraint.Create(pageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, scrollview.Superview, NSLayoutAttribute.Width, 1f, 0f),
                    NSLayoutConstraint.Create(pageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, scrollview, NSLayoutAttribute.Top, 1f, 0f)
                });

                if (i == 0)
                {
                    // no previous page
                    View.AddConstraint(NSLayoutConstraint.Create(pageView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, scrollview, NSLayoutAttribute.Left, 1f, 0f));
                }
                else
                {
                    // add constraint relative to previous page
                    View.AddConstraint(NSLayoutConstraint.Create(pageView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, scrollview.Subviews[i - 1], NSLayoutAttribute.Right, 1f, 0f));
                }

                if (i == (PageCount - 1))
                {
                    // last page
                    View.AddConstraint(NSLayoutConstraint.Create(pageView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, scrollview, NSLayoutAttribute.Right, 1f, 0f));
                }

                // constraints for labelTopTitle
                View.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(labelTopTitle, NSLayoutAttribute.Left, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(labelTopTitle, NSLayoutAttribute.Right, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(labelTopTitle, NSLayoutAttribute.Top, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.Top, 1f, 0f),
                });

                // constraints for labelTop
                View.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(labelTop, NSLayoutAttribute.Left, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(labelTop, NSLayoutAttribute.Right, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.Right, 1f, -12f),
                    NSLayoutConstraint.Create(labelTop, NSLayoutAttribute.Top, NSLayoutRelation.Equal, labelTopTitle, NSLayoutAttribute.Bottom, 1f, 5f),
                });

                // constraints for image
                View.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(image, NSLayoutAttribute.Width, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.Width, 1f, 0f),
                    NSLayoutConstraint.Create(image, NSLayoutAttribute.Height, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.Height, 1f, 0f),
                    NSLayoutConstraint.Create(image, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.CenterX, 1f, 0f),
                    NSLayoutConstraint.Create(image, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, pageView, NSLayoutAttribute.CenterY, 1f,
                        (UIScreen.MainScreen.Bounds.Height > minimalScreenHeight ? 30f : 60f)),
                });
            }

            pageControl.Hidden = false;
            pageControl.Pages = PageCount;
        }

        private void ScrollViewScrolled (object sender, EventArgs e)
        {
            var page = Math.Floor ((scrollview.ContentOffset.X - scrollview.Frame.Width / 2) / scrollview.Frame.Width) + 1;

            scrollview.ContentOffset = new CGPoint (scrollview.ContentOffset.X, 0);

            pageControl.CurrentPage = (int)page;
        }
    }
}

