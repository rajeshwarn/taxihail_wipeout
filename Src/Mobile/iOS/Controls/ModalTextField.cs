using System;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;
using Cirrious.MvvmCross.Dialog.Touch.Dialog;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Register("ModalTextField")]
    public class ModalTextField : TextFieldWithArrow
    {
        RootElement _rootElement;

        public ModalTextField(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        public ModalTextField(RectangleF rect) : base( rect )
        {
            Initialize();
        }

        private void Initialize() {
            EditingDidBegin += HandleEditingDidBegin;
        }

        void HandleEditingDidBegin (object sender, EventArgs e)
        {
            var controller = this.FindViewController();
            if(controller == null) return;
            controller.View.EndEditing(true);
            if (_rootElement != null) {
                var newDvc = new DialogViewController (_rootElement, true) {
                    Autorotate = true

                };
                newDvc.View.BackgroundColor  = UIColor.FromRGB (230,230,230);
                newDvc.TableView.BackgroundColor = UIColor.FromRGB (230,230,230);
                  
                //newDvc.TableView.  UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
//                apcurium.MK.Booking.Mobile.Client.Background.GetBackgroundView (
                newDvc.TableView.BackgroundView = new UIView{ BackgroundColor =  UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png")) }; 
                  
                controller.PresentViewController (newDvc, true, delegate { });
            }
        }

        public void Configure (string title, ListItem[] values, int selectedId, Action<ListItem> onItemSelected)
        {
            int selected = 0;
            var section = new SectionWithBackground(title);
            foreach (ListItem v in values)
            {
                // Keep a reference to value in order for callbacks to work correctly
                var value = v;
                var item = new RadioElementWithId(value.Id, value.Display);
                item.Tapped += ()=> {
                    onItemSelected(value);
                    var controller = this.FindViewController();
                    if(controller != null) controller.DismissViewController(true, delegate {});
                };
                section.Add(item);
                if (selectedId == value.Id)
                {
                    selected = Array.IndexOf(values, value);
                }
            }
            
            _rootElement = new CustomRootElement(Resources.RideSettingsVehiculeType, new RadioGroup(selected));
            _rootElement.Add(section);
        }


    }
}

