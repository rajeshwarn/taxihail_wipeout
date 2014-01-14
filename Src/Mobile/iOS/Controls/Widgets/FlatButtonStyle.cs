using MonoTouch.UIKit;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public abstract class FlatButtonStyle
	{
		//TO DELETE
		UIColor DarkYellow = UIColor.FromRGB(229, 184, 49);
		UIColor LightYellow = UIColor.FromRGB(255, 211, 80);

		UIColor Gray = UIColor.FromRGB(45, 45, 45);
		UIColor DarkGreen = UIColor.FromRGB(30, 192, 34);
		UIColor LightGreen = UIColor.FromRGB(49, 143, 51);
		UIColor DarkRed = UIColor.FromRGB(255, 0, 0);
		UIColor LightRed = UIColor.FromRGB(236, 28, 36);
		UIColor DarkBlue = UIColor.FromRGB(68, 118, 218);
		UIColor LightBlue = UIColor.FromRGB(111, 152, 235);


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

				UIColor Blue = UIColor.FromRGB(0, 71, 148);

				button.SetFillColor(Blue, UIControlState.Normal);
				button.SetFillColor(Blue.ColorWithAlpha(0.5f), UIControlState.Selected);
				button.SetFillColor(Blue.ColorWithAlpha(0.5f), UIControlState.Highlighted);

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

                var darkGray = UIColor.FromRGB(104, 104, 104);
                button.SetTitleColor(darkGray, UIControlState.Normal);
                button.SetTitleColor(darkGray.ColorWithAlpha(0.5f), UIControlState.Selected);
                button.SetTitleColor(darkGray.ColorWithAlpha(0.5f), UIControlState.Highlighted);
                button.SetTitleColor(darkGray.ColorWithAlpha(0.5f), UIControlState.Highlighted | UIControlState.Selected);

                button.SetStrokeColor(darkGray, UIControlState.Normal);
                button.SetStrokeColor(darkGray, UIControlState.Selected);
                button.SetStrokeColor(darkGray, UIControlState.Highlighted);

            }
        }

        private class GreenButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.SetFillColor(DarkGreen, UIControlState.Normal);
                button.SetFillColor(LightGreen, UIControlState.Selected);
                button.SetFillColor(LightGreen, UIControlState.Highlighted);

                button.SetTitleColor(UIColor.White, UIControlState.Normal);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Selected);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted | UIControlState.Selected);

                button.SetStrokeColor(UIColor.Clear, UIControlState.Normal);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Selected);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Highlighted);

            }
        }

        private class RedButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

				button.SetFillColor(DarkRed, UIControlState.Normal);
                button.SetFillColor(LightRed, UIControlState.Selected);
                button.SetFillColor(LightRed, UIControlState.Highlighted);

                button.SetTitleColor(UIColor.White, UIControlState.Normal);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Selected);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted);
                button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted | UIControlState.Selected);

                button.SetStrokeColor(UIColor.Clear, UIControlState.Normal);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Selected);
                button.SetStrokeColor(UIColor.Clear, UIControlState.Highlighted);

            }
        }

        private class ClearButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                if(button == null) return;

                button.SetFillColor(UIColor.Clear, UIControlState.Normal);
				button.SetFillColor(DarkYellow, UIControlState.Selected);
				button.SetFillColor(DarkYellow, UIControlState.Highlighted);

                button.SetTitleColor(UIColor.Black, UIControlState.Normal);
                button.SetTitleColor(UIColor.Black.ColorWithAlpha(0.5f), UIControlState.Selected);
                button.SetTitleColor(UIColor.Black.ColorWithAlpha(0.5f), UIControlState.Highlighted);
                button.SetTitleColor(UIColor.Black.ColorWithAlpha(0.5f), UIControlState.Highlighted | UIControlState.Selected);

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

