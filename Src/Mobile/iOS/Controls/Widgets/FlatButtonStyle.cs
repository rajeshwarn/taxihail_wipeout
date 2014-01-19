using MonoTouch.UIKit;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public abstract class FlatButtonStyle
	{
		UIColor MainColor = UIColor.FromRGB(0, 71, 148);
		UIColor Gray = UIColor.FromRGB(45, 45, 45);
		UIColor DarkGray = UIColor.FromRGB(104, 104, 104);
		UIColor DarkBlue = UIColor.FromRGB(68, 118, 218);
		UIColor LightBlue = UIColor.FromRGB(111, 152, 235);
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
        public static readonly FlatButtonStyle Social = new SocialButtonStyle();
        public static readonly FlatButtonStyle Clear = new ClearButtonStyle();
        public static readonly FlatButtonStyle Utility = new UtilityButtonStyle();
        public static readonly FlatButtonStyle Green = new GreenButtonStyle();
        public static readonly FlatButtonStyle Red = new RedButtonStyle();

        private class MainButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.SetFillColor(MainColor, UIControlState.Normal);
				button.SetFillColor(MainColor.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetFillColor(MainColor.ColorWithAlpha(0.5f), UIControlState.Highlighted);

				button.SetTitleColor(UIColor.White, UIControlState.Normal);
				button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetTitleColor(UIColor.White, UIControlState.Highlighted);

                button.SetStrokeColor(UIColor.Black, UIControlState.Normal);
                button.SetStrokeColor(UIColor.Black, UIControlState.Selected);
                button.SetStrokeColor(UIColor.Black, UIControlState.Highlighted);
            }
        }

        private class UtilityButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

                button.SetFillColor(UIColor.White, UIControlState.Normal);
				button.SetFillColor(Gray, UIControlState.Selected);
				button.SetFillColor(Gray, UIControlState.Highlighted);

				button.SetTitleColor(DarkGray, UIControlState.Normal);
				button.SetTitleColor(DarkGray.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetTitleColor(DarkGray.ColorWithAlpha(0.5f), UIControlState.Highlighted);
				button.SetTitleColor(DarkGray.ColorWithAlpha(0.5f), UIControlState.Highlighted | UIControlState.Selected);

				button.SetStrokeColor(DarkGray, UIControlState.Normal);
				button.SetStrokeColor(DarkGray, UIControlState.Selected);
				button.SetStrokeColor(DarkGray, UIControlState.Highlighted);
            }
        }

        private class GreenButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.SetFillColor(UIColor.Clear, UIControlState.Normal);
				button.SetFillColor(UIColor.Clear, UIControlState.Selected);
				button.SetFillColor(UIColor.Clear, UIControlState.Highlighted);

				button.SetTitleColor(GreenColor, UIControlState.Normal);
				button.SetTitleColor(GreenColor.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetTitleColor(GreenColor.ColorWithAlpha(0.5f), UIControlState.Highlighted);

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
				button.SetFillColor(UIColor.Clear, UIControlState.Selected);
				button.SetFillColor(UIColor.Clear, UIControlState.Highlighted);

				button.SetTitleColor(UIColor.Red, UIControlState.Normal);
				button.SetTitleColor(UIColor.Red.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetTitleColor(UIColor.Red.ColorWithAlpha(0.5f), UIControlState.Highlighted);

				button.SetStrokeColor(UIColor.Red, UIControlState.Normal);
				button.SetStrokeColor(UIColor.Red, UIControlState.Selected);
				button.SetStrokeColor(UIColor.Red, UIControlState.Highlighted);
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

				button.SetTitleColor(UIColor.White, UIControlState.Normal);
				button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted);

                button.SetStrokeColor(UIColor.Clear, UIControlState.Normal);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Selected);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Highlighted);
            }
        }

        private class SocialButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

                button.SetFillColor(LightBlue, UIControlState.Normal);
                button.SetFillColor(DarkBlue, UIControlState.Selected);
                button.SetFillColor(DarkBlue, UIControlState.Highlighted);

                button.SetTitleColor(UIColor.White, UIControlState.Normal);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Selected);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted | UIControlState.Selected);

                button.SetStrokeColor(UIColor.Black, UIControlState.Normal);
                button.SetStrokeColor(UIColor.Black, UIControlState.Selected);
                button.SetStrokeColor(UIColor.Black, UIControlState.Highlighted);
            }
        }
    }
}

