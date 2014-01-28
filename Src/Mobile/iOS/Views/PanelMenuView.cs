using System;
using System.ComponentModel;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Animations;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class PanelMenuView : MvxView
    {
		private const string Cellid = "PanelMenuCell";

		private const string CellBindingText = @"
                   TitleText Text;
                   SelectedCommand NavigationCommand;
                ";

		public UIView ViewToAnimate
		{
			get;
			set;
		}

		private bool _menuIsOpen;
		public bool MenuIsOpen
		{
			get { return _menuIsOpen; }
			set
			{
				if (_menuIsOpen != value)
				{
					_menuIsOpen = value;
					AnimateMenu ();
				}
			}
		}

        public ICommand ToApcuriumWebsite { get; set; }
        public ICommand ToMobileKnowledgeWebsite { get; set; }

		public PanelMenuView (IntPtr handle)
			:base(handle)
        {
			this.DelayBind (() => {
				InitializeMenu();
			});
        }

        private void InitializeMenu ()
        {
			menuContainer.BackgroundColor = UIColor.FromRGB (242, 242, 242);

            var sideLine = Line.CreateVertical(menuContainer.Frame.Width, Frame.Height, UIColor.FromRGB(190, 190, 190));
            AddSubview(sideLine);

			var source = new PanelMenuSource(
				menuListView, 
				UITableViewCellStyle.Default,
				new NSString(Cellid), 
				CellBindingText,
				UITableViewCellAccessory.None);

			menuListView.Source = source;

			var set = this.CreateBindingSet<PanelMenuView, PanelMenuViewModel>();

			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.ItemMenuList);

			set.Bind (this)
				.For (v => v.MenuIsOpen)
				.To (vm => vm.MenuIsOpen);

            set.Bind (lblVersion)
                .For (v => v.Text)
                .To (vm => vm.Version);

            set.Bind(this)
                .For(v => v.ToApcuriumWebsite)
                .To(vm => vm.ToApcuriumWebsite);

            set.Bind(this)
                .For(v => v.ToMobileKnowledgeWebsite)
                .To(vm => vm.ToMobileKnowledgeWebsite);

			set.Apply ();

            imgLogoApcurium.AddGestureRecognizer(new UITapGestureRecognizer(() =>
                {
                    if(ToApcuriumWebsite != null && ToApcuriumWebsite.CanExecute())
                    {
                        ToApcuriumWebsite.Execute();
                    }
                }));

            imgLogoMobileKnowledge.AddGestureRecognizer(new UITapGestureRecognizer(() =>
                {
                    if(ToMobileKnowledgeWebsite != null && ToMobileKnowledgeWebsite.CanExecute())
                    {
                        ToMobileKnowledgeWebsite.Execute();
                    }
                }));
		
			menuListView.AlwaysBounceVertical = false;
        }

        private void AnimateMenu ()
        {
            InvokeOnMainThread (() =>
            {
				var slideAnimation = new SlideViewAnimation (ViewToAnimate, new SizeF ((MenuIsOpen ? menuContainer.Frame.Width : -menuContainer.Frame.Width), 0f));
                slideAnimation.Animate ();
            });
        }
    }
}


