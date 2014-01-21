using System;
using System.Drawing;
using System.Globalization;
using CrossUI.Touch.Dialog;
using CrossUI.Touch.Dialog.Elements;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.MonoTouchDialog;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("ModalFlatTextField")]
    public class ModalFlatTextField : FlatTextField
    {
        private RootElement _rootElement;
        private UIButton _button;

        public ModalFlatTextField (IntPtr handle) : base (handle)
        {
            Initialize();
        }

        public ModalFlatTextField () : base()
        {
            Initialize();
        }

        public ModalFlatTextField (RectangleF frame) : base (frame)
        {
            Initialize();
        }

        private void Initialize ()
        {
            HasRightArrow = true;

            _button = new UIButton(Bounds);
            AddSubview(_button);
        }

        protected UIButton Button
        { 
            get {
                return _button;
            }
        }

        public override void WillMoveToSuperview (UIView newsuper)
        {
            base.WillMoveToSuperview (newsuper);

            Button.TouchUpInside -= HandleTouchUpInside;
            Button.TouchUpInside += HandleTouchUpInside;
        }

        void HandleTouchUpInside (object sender, EventArgs e)
        {
            var controller = this.FindViewController();
            if(controller == null) return;
            controller.View.EndEditing(true);
            if (_rootElement != null) {
                var newDvc = new TaxiHailDialogViewController (_rootElement, true, false);
                controller.NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);
                controller.NavigationController.PushViewController(newDvc, true);
            }
        }

        public void Configure<T>(string title, ListItem<T>[] values,  Nullable<T> selectedId, Action<ListItem<T>> onItemSelected ) where T: struct
        {
            if ( values == null )
            {
                return;
            }

            var selected = 0;
            var section = new Section();

            foreach (var v in values)
            {
                // Keep a reference to value in order for callbacks to work correctly
                var value = v;
                var display = value.Display;
                if (!value.Id.HasValue)
                {
                    display = Localize.GetValue("NoPreference");
                }

                var item = new RadioElementWithId<T>(value.Id, display, value.Image, values.Last() != value);
                item.Tapped += () =>
                {
                    onItemSelected(value);
                    var controller = this.FindViewController();
                    if (controller != null)
                        controller.NavigationController.PopViewControllerAnimated(true);
                };
                section.Add(item);
                if (selectedId.Equals(value.Id))
                {
                    selected = Array.IndexOf(values, value);
                }
            }

            _rootElement = new RootElement(title, new RadioGroup(selected));
            _rootElement.Add(section);
        }
    }
}

