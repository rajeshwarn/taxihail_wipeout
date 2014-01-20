using MonoTouch.UIKit;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public abstract class FlatButtonStyle
	{
		UIColor MainColor = UIColor.FromRGB(0, 71, 148);
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
				button.SetTitleColor(UIColor.White.ColorWithAlpha(0.5f), UIControlState.Highlighted);

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
    }
}

