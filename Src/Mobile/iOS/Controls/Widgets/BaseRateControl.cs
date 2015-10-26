using System;
using Foundation;
using UIKit;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("BaseRateView")]
	public class BaseRateControl : UIControl
    {
		private UILabel[] _labels;
		private UILabel[] _descriptions;

        public BaseRateControl()
        {
            ClipsToBounds = true;
			BackgroundColor = UIColor.White;
			TranslatesAutoresizingMaskIntoConstraints = false;
        }

        void Initialize ()
		{
			_labels = new UILabel[5];
			_descriptions = new UILabel[5];
			Subviews.ForEach (x => x.RemoveFromSuperview ());

			if (BaseRate == null)
			{
				return;
			}

			var labelsText = new [] 
            { 
                this.Services().Localize["BaseRate_MinimumFare"],  
                this.Services().Localize["BaseRate_BaseRate"], 
                this.Services().Localize["BaseRate_PerMileRate"], 
                this.Services().Localize["BaseRate_WaitTime"], 
                this.Services().Localize["BaseRate_AirportMeetAndGreet"]
			};

			var descriptionsText = new [] 
            { 
				ToCurrency (BaseRate.MinimumFare),
				ToCurrency (BaseRate.BaseRateNoMiles), 
				string.Format (Localize ("BaseRate_PerTenthMile"), ToCurrency (BaseRate.PerMileRate), ToCurrency (BaseRate.PerMileRate / 10)),
				string.Format (Localize ("BaseRate_PerMinute"), ToCurrency (BaseRate.WaitTime)),
				ToCurrency (BaseRate.AirportMeetAndGreet)
			};

			for (int i = 0; i < 5; i++)
			{
				_labels [i] = new UILabel
				{ 
					TranslatesAutoresizingMaskIntoConstraints = false,
					Text = labelsText [i],
					TextColor = UIColor.Black,
					Font = UIFont.FromName (FontName.HelveticaNeueBold, 13.0f)
				};

				_descriptions [i] = new UILabel
				{ 
					TranslatesAutoresizingMaskIntoConstraints = false,
					Text = descriptionsText [i],
					TextColor = UIColor.Black,
					Font = UIFont.FromName (FontName.HelveticaNeueRegular, 13.0f)
				};

				AddSubviews (_labels [i], _descriptions [i]);

				AddConstraints (new []
				{
					NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Left, NSLayoutRelation.Equal, _labels [i].Superview, NSLayoutAttribute.Left, 1f, 12f),
					NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Width, NSLayoutRelation.Equal, _labels [i].Superview, NSLayoutAttribute.Width, 0.45f, 0f),
					NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 18f),
					NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Right, NSLayoutRelation.Equal, _descriptions [i].Superview, NSLayoutAttribute.Right, 1f, -12f),
					NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Width, NSLayoutRelation.Equal, _descriptions [i].Superview, NSLayoutAttribute.Width, 0.45f, 0f),
					NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 18f)
				});

				if (i == 0)
				{
					AddConstraint (NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Top, NSLayoutRelation.Equal, _labels [i].Superview, NSLayoutAttribute.Top, 1f, 4f));
					AddConstraint (NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Top, NSLayoutRelation.Equal, _descriptions [i].Superview, NSLayoutAttribute.Top, 1f, 4f));
				} 
                else
				{
					AddConstraint (NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Top, NSLayoutRelation.Equal, _labels [i - 1], NSLayoutAttribute.Bottom, 1f, 0f));
					AddConstraint (NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Top, NSLayoutRelation.Equal, _descriptions [i - 1], NSLayoutAttribute.Bottom, 1f, 0f));
				}
			}
			this.LayoutSubviews();
        }

        string ToCurrency(decimal amount)
        {
			return CultureProvider.FormatCurrency((float) amount);
        }

        string Localize(string key) 
        {
        	return this.Services().Localize[key];
        }

		public string CurrencySymbol { get; set; }

		private BaseRateInfo _baseRate;
		public BaseRateInfo BaseRate
		{
			get
			{
				return _baseRate; 
			}
			set
			{ 
				_baseRate = value;
				Initialize();
			}
		}
    }
}