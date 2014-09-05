using MonoTouch.UIKit;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using System.Xml;
using System.Xml.Serialization;
using apcurium.MK.Common.Extensions;
using System;
using apcurium.MK.Booking.Mobile.Client.Helper;

namespace apcurium.MK.Booking.Mobile.Client
{
	public abstract class FlatButtonStyle
	{
        private FlatButtonStyles _flatButtonStyles;

        private readonly UIColor MainColor = UIColor.Black.ColorWithAlpha(0.15f);
        private readonly UIColor DarkBlue = UIColor.FromRGB(3, 27, 49);
        private readonly UIColor GreenColor = UIColor.FromRGB(31, 191, 33);
        private readonly UIColor SilverColor = UIColor.FromRGB(90, 90, 90);

    	public static UIFont ClearButtonFont = UIFont.FromName(FontName.HelveticaNeueBold, 28/2);

        public virtual void ApplyTo(FlatButton button)
        {
            if (_flatButtonStyles == null)
            {
                using (var styleFile = typeof(FlatButtonStyles).Assembly.GetManifestResourceStream("FlatButtonStyle.xml"))
                {
                    var serializer = new XmlSerializer(typeof(FlatButtonStyles));
                    _flatButtonStyles = (FlatButtonStyles)serializer.Deserialize(styleFile);
                }
            }
        }

        public void ApplyTo (params UIButton[] buttons)
        {
            foreach (var button in buttons.OfType<FlatButton>()) {
                ApplyTo(button);
            }
        }

        protected UIColor GetColor(Func<FlatButtonStyles.Color, UIColor> selector, UIColor defaultColor)
        {
            if (_flatButtonStyles == null)
            {
                return null;
            }

            return _flatButtonStyles.Colors
                .FirstOrDefault (x => x.Name == GetStyleName())
                .SelectOrDefault(selector, defaultColor);
        }

        protected abstract string GetStyleName();

		public static readonly FlatButtonStyle Default = new DefaultButtonStyle ();
        public static readonly FlatButtonStyle Main = new MainButtonStyle(); //ok button
        public static readonly FlatButtonStyle Clear = new ClearButtonStyle();
        public static readonly FlatButtonStyle Green = new GreenButtonStyle();
        public static readonly FlatButtonStyle Red = new RedButtonStyle();
        public static readonly FlatButtonStyle Silver = new SilverButtonStyle();

		private class DefaultButtonStyle: FlatButtonStyle
		{
			public override void ApplyTo (FlatButton button)
			{
                base.ApplyTo (button);

				if(button == null) return;

                button.SetFillColor(GetColor(x => x.ColorBackgroundNormal, UIColor.Clear), UIControlState.Normal);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, UIColor.Clear), UIControlState.Selected);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, UIColor.Clear), UIControlState.Highlighted);

                button.SetTitleColor(GetColor(x => x.ColorTextNormal, Theme.ButtonTextColor), UIControlState.Normal);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, Theme.ButtonTextColor.ColorWithAlpha(0.5f)), UIControlState.Selected);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, Theme.ButtonTextColor.ColorWithAlpha(0.5f)), UIControlState.Highlighted);

                button.SetStrokeColor(GetColor(x => x.ColorTextNormal, Theme.ButtonTextColor));
			}

            protected override string GetStyleName ()
            {
                return "Default";
            }
		}

        private class MainButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                base.ApplyTo (button);

                if(button == null) return;

                button.SetFillColor(GetColor(x => x.ColorBackgroundNormal, MainColor), UIControlState.Normal);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, MainColor), UIControlState.Selected);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, MainColor), UIControlState.Highlighted);

                button.SetTitleColor(GetColor(x => x.ColorTextNormal, Theme.ButtonTextColor), UIControlState.Normal);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, Theme.ButtonTextColor.ColorWithAlpha(0.5f)), UIControlState.Selected);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, Theme.ButtonTextColor.ColorWithAlpha(0.5f)), UIControlState.Highlighted);

				button.SetStrokeColor(DarkBlue);
            }

            protected override string GetStyleName ()
            {
                return "Main";
            }
        }

        private class GreenButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                base.ApplyTo (button);

                if(button == null) return;

                button.SetFillColor(GetColor(x => x.ColorBackgroundNormal, UIColor.Clear), UIControlState.Normal);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, GreenColor), UIControlState.Selected);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, GreenColor), UIControlState.Highlighted);

                button.SetTitleColor(GetColor(x => x.ColorTextNormal, GreenColor), UIControlState.Normal);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, UIColor.White), UIControlState.Selected);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, UIColor.White), UIControlState.Highlighted);

				button.SetStrokeColor(GreenColor);
            }

            protected override string GetStyleName ()
            {
                return "Green";
            }
        }

        private class RedButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                base.ApplyTo (button);

                if(button == null) return;

                button.SetFillColor(GetColor(x => x.ColorBackgroundNormal, UIColor.Clear), UIControlState.Normal);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, UIColor.Red), UIControlState.Selected);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, UIColor.Red), UIControlState.Highlighted);

                button.SetTitleColor(GetColor(x => x.ColorTextNormal, UIColor.Red), UIControlState.Normal);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, UIColor.White), UIControlState.Selected);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, UIColor.White), UIControlState.Highlighted);

                button.SetStrokeColor(UIColor.Red);
            }

            protected override string GetStyleName ()
            {
                return "Red";
            }
        }

        private class SilverButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                base.ApplyTo (button);

                if(button == null) return;

                button.SetFillColor(GetColor(x => x.ColorBackgroundNormal, UIColor.Clear), UIControlState.Normal);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, SilverColor), UIControlState.Selected);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, SilverColor), UIControlState.Highlighted);

                button.SetTitleColor(GetColor(x => x.ColorTextNormal, SilverColor), UIControlState.Normal);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, UIColor.White), UIControlState.Selected);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, UIColor.White), UIControlState.Highlighted);

                button.SetStrokeColor(SilverColor);
            }

            protected override string GetStyleName ()
            {
                return "Silver";
            }
        }

        private class ClearButtonStyle: FlatButtonStyle
        {
            public override void ApplyTo (FlatButton button)
            {
                base.ApplyTo (button);

                if(button == null) return;

				button.Font = ClearButtonFont;

                button.SetFillColor(GetColor(x => x.ColorBackgroundNormal, UIColor.Clear), UIControlState.Normal);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, UIColor.Clear), UIControlState.Selected);
                button.SetFillColor(GetColor(x => x.ColorBackgroundSelected, UIColor.Clear), UIControlState.Highlighted);

                button.SetTitleColor(GetColor(x => x.ColorTextNormal, Theme.ButtonTextColor), UIControlState.Normal);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, Theme.ButtonTextColor.ColorWithAlpha(0.5f)), UIControlState.Selected);
                button.SetTitleColor(GetColor(x => x.ColorTextSelected, Theme.ButtonTextColor.ColorWithAlpha(0.5f)), UIControlState.Highlighted);

                button.SetStrokeColor(UIColor.Clear);
            }

            protected override string GetStyleName ()
            {
                return "Clear";
            }
        }

        public class FlatButtonStyles
        {
            public Color[] Colors { get; set; }

            public class Color
            {
                [XmlAttribute("name")]
                public string Name { get; set; }

                public string BackgroundNormal { get; set; }
                public string BackgroundSelected { get; set; }
                public string TextNormal { get; set; }
                public string TextSelected { get; set; }

                private UIColor _backgroundNormal;
                public UIColor ColorBackgroundNormal { get { return ColorHelper.ToUIColor (BackgroundNormal, ref _backgroundNormal); } }
            
                private UIColor _backgroundSelected;
                public UIColor ColorBackgroundSelected { get { return ColorHelper.ToUIColor (BackgroundSelected, ref _backgroundSelected); } }

                private UIColor _textNormal;
                public UIColor ColorTextNormal { get { return ColorHelper.ToUIColor (TextNormal, ref _textNormal); } }

                private UIColor _textSelected;
                public UIColor ColorTextSelected { get { return ColorHelper.ToUIColor (TextSelected, ref _textSelected); } }
            }
        }
    }
}

