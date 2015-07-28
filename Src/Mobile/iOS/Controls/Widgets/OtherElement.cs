using System;
using CrossUI.Touch.Dialog.Elements;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OtherElement : EntryElement
    {
        public OtherElement() : base (null, Localize.GetValue("OtherListItemLabel"))
        {
            
        }

        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell = base.GetCellImpl (tv);

            cell.Frame  = cell.Frame.SetHeight(tv.RowHeight);
            cell.ContentView.Frame = cell.ContentView.Frame.SetHeight(tv.RowHeight);

            cell.BackgroundColor = UIColor.Clear;
            cell.ContentView.BackgroundColor = UIColor.Clear;
            this.Alignment = NaturalLanguageHelper.GetTextAlignment();

            cell.BackgroundView = new CustomCellBackgroundView(cell.ContentView.Frame, 10, UIColor.White, UIColor.FromRGB(190, 190, 190)) 
                {
                    HideBottomBar = true
                };

            return cell;
        }

        protected override UITextField CreateTextField(CoreGraphics.CGRect frame)
        {
            var textField = base.CreateTextField(frame);
            textField.LeftView = new UIView(new CGRect (0, 0, 15, frame.Height));
            textField.Font = UIFont.FromName(FontName.HelveticaNeueLight, 32/2);
            textField.TextColor = UIColor.FromRGB(44, 44, 44);
            textField.LeftViewMode = UITextFieldViewMode.Always;
            return textField;
        }
    }
}

