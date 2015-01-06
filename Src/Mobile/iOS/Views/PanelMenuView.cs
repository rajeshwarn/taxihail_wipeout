using System;
using System.Drawing;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class PanelMenuView : BaseBindableView<PanelMenuViewModel>
    {
        private const string CellId = "PanelMenuCell";
		private const string CellBindingText = @"
                   TitleText Text;
                   SelectedCommand NavigationCommand;
                ";
        private UIButton _closeMenuButton;
        private PanelMenuSource _source;

        public UIView ViewToAnimate { get; set; }
        public NSLayoutConstraint PanelOffsetConstraint { get; set; }
        public ICommand ToApcuriumWebsite { get; set; }
        public ICommand ToMobileKnowledgeWebsite { get; set; }

        public PanelMenuView (IntPtr handle) : base(handle)
        {
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

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var nib = NibHelper.GetNibForView("PanelMenuView");
            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            OnInstantiate();

            this.DelayBind (() => {
                InitializeMenu();
            });
        }

		public void OnInstantiate ()
		{
			_source = new PanelMenuSource (menuListView, UITableViewCellStyle.Default, 
											new NSString (CellId), 
											CellBindingText, UITableViewCellAccessory.None);
			menuListView.Source = _source;
		}

        private void InitializeMenu ()
        {
            menuContainer.BackgroundColor = Theme.MenuColor;

            lblVersion.TextColor = Theme.ShouldHaveLightContent(Theme.MenuColor)
                ? UIColor.White
                : UIColor.FromRGB (79, 76, 71);

            var sideLine = Line.CreateVertical(menuContainer.Frame.Width, Frame.Height, UIColor.FromRGB(190, 190, 190));
            AddSubview(sideLine);

			var set = this.CreateBindingSet<PanelMenuView, PanelMenuViewModel>();

			set.Bind(_source)
				.For(v => v.ItemsSource)
				.To(vm => vm.ItemMenuList);

			set.Bind ()
				.For (v => v.MenuIsOpen)
                .To (vm => vm.MenuIsOpen);

            set.Bind (lblVersion)
                .For (v => v.Text)
                .To (vm => vm.Version);

            set.Bind()
                .For(v => v.ToApcuriumWebsite)
                .To(vm => vm.ToApcuriumWebsite);

            set.Bind()
                .For(v => v.ToMobileKnowledgeWebsite)
                .To(vm => vm.ToMobileKnowledgeWebsite);

			set.Apply ();

			if (ViewModel.Settings.HideMkApcuriumLogos)
			{
				imgLogoApcurium.Hidden = true;
				imgLogoMobileKnowledge.Hidden = true;
			}
            else
            {
				imgLogoApcurium.AddGestureRecognizer (new UITapGestureRecognizer (() => ToApcuriumWebsite.ExecuteIfPossible()));
                imgLogoMobileKnowledge.AddGestureRecognizer (new UITapGestureRecognizer (() => ToMobileKnowledgeWebsite.ExecuteIfPossible()));
			}

			menuListView.AlwaysBounceVertical = false;
        }

        private void AnimateMenu ()
        {
            InvokeOnMainThread (() =>
                UIView.Animate(0.5f, () => {
                    PanelOffsetConstraint.Constant = MenuIsOpen ? menuContainer.Frame.Width : 0;
                    ViewToAnimate.LayoutIfNeeded();
                    this.SetNeedsDisplay();
                }, AddOrRemoveInvisibleCloseButton)
            );
        }

        private void AddOrRemoveInvisibleCloseButton()
        {
            if(MenuIsOpen && _closeMenuButton == null)
            {
                _closeMenuButton = new UIButton(new RectangleF(0, 0, Frame.Width - menuContainer.Frame.Width, Frame.Height)) { BackgroundColor = UIColor.Clear };
                _closeMenuButton.TouchUpInside += (s, ex) => {
                    ViewModel.MenuIsOpen = false;
                };
                ViewToAnimate.AddSubview(_closeMenuButton);
            }
            else
            {
                if (_closeMenuButton != null)
                {
                    _closeMenuButton.RemoveFromSuperview();
                    _closeMenuButton = null;
                }
            }
        }
    }
}


