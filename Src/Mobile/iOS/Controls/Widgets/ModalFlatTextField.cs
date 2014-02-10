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
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

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
            get { return _button; }
        }

        public void Configure<T>(string title, Func<ListItem<T>[]> getValues, Func<Nullable<T>> selectedId, Action<ListItem<T>> onItemSelected) where T : struct
        {
            Button.TouchUpInside += (sender, e) => {
                var values = getValues();

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
                    if (selectedId().Equals(value.Id))
                    {
                        selected = Array.IndexOf(values, value);
                    }
                }

                _rootElement = new RootElement(title, new RadioGroup(selected));
                _rootElement.Add(section);

                var currentController = this.FindViewController();
                if(currentController == null) return;
                currentController.View.EndEditing(true);

                var newDvc = new TaxiHailDialogViewController (_rootElement, true, false);
                currentController.NavigationController.NavigationBar.Hidden = false;
                currentController.NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);
                currentController.NavigationController.PushViewController(newDvc, true);
            };
        }
    }
}

