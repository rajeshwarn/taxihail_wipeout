using MonoTouch.UIKit;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public abstract class FlatButtonStyle
	{
		UIColor MainColor = UIColor.Black.ColorWithAlpha(0.15f);
		UIColor DarkBlue = UIColor.FromRGB(3, 27, 49);
		UIColor GreenColor = UIColor.FromRGB(31, 191, 33);

		public static UIFont ClearButtonFont = UIFont.FromName(FontName.HelveticaNeueBold, 28/2);

		public abstract void ApplyTo(FlatButton button);

        public void ApplyTo (params UIButton[] buttons)
        {
            foreach (var button in buttons.OfType<FlatButton>()) {
                ApplyTo(button);
            }
        }

        public static readonly FlatButtonStyle Main = new MainButtonStyle(); //ok button
        public static readonly FlatButtonStyle Clear = new ClearButtonStyle();
        public static readonly FlatButtonStyle Green = new GreenButtonStyle();
        public static readonly FlatButtonStyle Red = new RedButtonStyle();
        public static readonly FlatButtonStyle Silver = new SilverButtonStyle();

        private class MainButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.SetFillColor(MainColor, UIControlState.Normal);
				button.SetFillColor(MainColor, UIControlState.Selected);
				button.SetFillColor(MainColor, UIControlState.Highlighted);

				button.SetTitleColor(Theme.ButtonTextColor, UIControlState.Normal);
				button.SetTitleColor(Theme.ButtonTextColor.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetTitleColor(Theme.ButtonTextColor.ColorWithAlpha(0.5f), UIControlState.Highlighted);

				button.SetStrokeColor(DarkBlue, UIControlState.Normal);
				button.SetStrokeColor(DarkBlue.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetStrokeColor(DarkBlue.ColorWithAlpha(0.5f), UIControlState.Highlighted);
            }
        }

        private class GreenButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.SetFillColor(UIColor.Clear, UIControlState.Normal);
				button.SetFillColor(GreenColor, UIControlState.Selected);
				button.SetFillColor(GreenColor, UIControlState.Highlighted);

				button.SetTitleColor(GreenColor, UIControlState.Normal);
				button.SetTitleColor(UIColor.White, UIControlState.Selected);
				button.SetTitleColor(UIColor.White, UIControlState.Highlighted);

				button.SetStrokeColor(GreenColor, UIControlState.Normal);
				button.SetStrokeColor(GreenColor.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetStrokeColor(GreenColor.ColorWithAlpha(0.5f), UIControlState.Highlighted);
            }
        }

        private class RedButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.SetFillColor(UIColor.Clear, UIControlState.Normal);
				button.SetFillColor(UIColor.Red, UIControlState.Selected);
				button.SetFillColor(UIColor.Red, UIControlState.Highlighted);

                button.SetTitleColor(UIColor.Red, UIControlState.Normal);
				button.SetTitleColor(UIColor.White, UIControlState.Selected);
				button.SetTitleColor(UIColor.White, UIControlState.Highlighted);

                button.SetStrokeColor(UIColor.Red, UIControlState.Normal);
				button.SetStrokeColor(UIColor.Red.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetStrokeColor(UIColor.Red.ColorWithAlpha(0.5f), UIControlState.Highlighted);
            }
        }

        private class SilverButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

                button.SetFillColor(UIColor.Clear, UIControlState.Normal);
				button.SetFillColor(UIColor.FromRGB(90, 90, 90), UIControlState.Selected);
				button.SetFillColor(UIColor.FromRGB(90, 90, 90), UIControlState.Highlighted);

                button.SetTitleColor(UIColor.FromRGB(90, 90, 90), UIControlState.Normal);
				button.SetTitleColor(UIColor.White, UIControlState.Selected);
				button.SetTitleColor(UIColor.White, UIControlState.Highlighted);

                button.SetStrokeColor(UIColor.FromRGB(90, 90, 90), UIControlState.Normal);
				button.SetStrokeColor(UIColor.FromRGB(90, 90, 90).ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetStrokeColor(UIColor.FromRGB(90, 90, 90).ColorWithAlpha(0.5f), UIControlState.Highlighted);
            }
        }

        private class ClearButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.Font = ClearButtonFont;

                button.SetFillColor(UIColor.Clear, UIControlState.Normal);
				button.SetFillColor(UIColor.Clear, UIControlState.Selected);
				button.SetFillColor(UIColor.Clear, UIControlState.Highlighted);

				button.SetTitleColor(Theme.ButtonTextColor, UIControlState.Normal);
				button.SetTitleColor(Theme.ButtonTextColor.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetTitleColor(Theme.ButtonTextColor.ColorWithAlpha(0.5f), UIControlState.Highlighted);

                button.SetStrokeColor(UIColor.Clear, UIControlState.Normal);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Selected);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Highlighted);
            }
        }
    }
}

