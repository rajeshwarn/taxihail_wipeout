using System;
using Foundation;
using UIKit;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("BaseRateView")]
	public class BaseRateControl : UIControl
    {
    	public const float ItemHeight = 18f;
		public const float ItemVPadding = 4f;
		public const float ItemHPadding = 12f;
		public const float LabelHWeight = 0.45f;

		public const int NumberOfItems = 4;

		private UILabel[] _labels;
		private UILabel[] _descriptions;
		private UILabel _baseRateTitleLabel;

        public BaseRateControl()
        {
            ClipsToBounds = true;
			BackgroundColor = UIColor.White;
			TranslatesAutoresizingMaskIntoConstraints = false;
        }

        private void Initialize ()
		{
			_labels = new UILabel[NumberOfItems];
			_descriptions = new UILabel[NumberOfItems];
			Subviews.ForEach (x => x.RemoveFromSuperview ());

			if (BaseRate == null)
			{
				return;
			}

			var labelsText = new [] { 
				this.Services ().Localize ["BaseRate_MinimumFare"],  
				this.Services ().Localize ["BaseRate_BaseRate"], 
				this.Services ().Localize ["BaseRate_PerMileRate"], 
				this.Services ().Localize ["BaseRate_WaitTime"]
			};

			var mileageRateText = ServiceType == ServiceType.Taxi ? Localize ("BaseRate_PerQuarterMile") : Localize ("BaseRate_PerTenthMile");
			var mileageRateAmount = ServiceType == ServiceType.Taxi ? ToCurrency (BaseRate.PerMileRate / 4): ToCurrency (BaseRate.WaitTime);

			var waitTimeText = ServiceType == ServiceType.Taxi ? Localize ("BaseRate_PerEightySeconds") : Localize ("BaseRate_PerMinute");
			var waitTimeAmount = ServiceType == ServiceType.Taxi ? ToCurrency(BaseRate * 1.3333333333m) : ToCurrency (BaseRate.WaitTime);

			var descriptionsText = BaseRate != null 
				? new [] { 
				ToCurrency (BaseRate.MinimumFare),
				ToCurrency (BaseRate.BaseRateNoMiles), 
				string.Format (mileageRateText, ToCurrency (BaseRate.PerMileRate), mileageRateAmount),
				string.Format (waitTimeText, waitTimeAmount)
			} : new string[5];

			for (int i = NumberOfItems - 1; i > -1; i--)
			{	
				_labels [i] = new UILabel { 
					TranslatesAutoresizingMaskIntoConstraints = false,
					Text = labelsText [i],
					TextColor = UIColor.Black,
					Font = UIFont.FromName (FontName.HelveticaNeueRegular, 13.0f)
				};

				_descriptions [i] = new UILabel {
					TranslatesAutoresizingMaskIntoConstraints = false,
					Text = descriptionsText [i],
					TextColor = UIColor.Black,
					Font = UIFont.FromName (FontName.HelveticaNeueRegular, 13.0f)
				};

				AddSubviews (_labels [i], _descriptions [i]);

				AddConstraints (new [] {
					NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Left, NSLayoutRelation.Equal, _labels [i].Superview, NSLayoutAttribute.Left, 1f, ItemHPadding),
					NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Width, NSLayoutRelation.Equal, _labels [i].Superview, NSLayoutAttribute.Width, LabelHWeight, 0f),
					NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ItemHeight),
					NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Right, NSLayoutRelation.Equal, _descriptions [i].Superview, NSLayoutAttribute.Right, 1f, -ItemHPadding),
					NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Width, NSLayoutRelation.Equal, _descriptions [i].Superview, NSLayoutAttribute.Width, LabelHWeight, 0f),
					NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ItemHeight)
				});

				if (i == NumberOfItems - 1)
				{
					AddConstraint (NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _labels [i].Superview, NSLayoutAttribute.Bottom, 1f, -ItemVPadding));
					AddConstraint (NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _descriptions [i].Superview, NSLayoutAttribute.Bottom, 1f, -ItemVPadding));
				} else
				{
					AddConstraint (NSLayoutConstraint.Create (_labels [i], NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _labels [i + 1], NSLayoutAttribute.Bottom, 1f, -ItemHeight));
					AddConstraint (NSLayoutConstraint.Create (_descriptions [i], NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _descriptions [i + 1], NSLayoutAttribute.Bottom, 1f, -ItemHeight));
				}
			}

			_baseRateTitleLabel = new UILabel {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Text = Localize ("BaseRate_RateStructure"),
				TextColor = UIColor.Black,
				Font = UIFont.FromName (FontName.HelveticaNeueBold, 13.0f)
			};

			AddSubview (_baseRateTitleLabel);

			AddConstraints (new [] {
				NSLayoutConstraint.Create (_baseRateTitleLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ItemHeight),
				NSLayoutConstraint.Create (_baseRateTitleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, _baseRateTitleLabel.Superview, NSLayoutAttribute.Left, 1f, ItemHPadding),
				NSLayoutConstraint.Create (_baseRateTitleLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, _baseRateTitleLabel.Superview, NSLayoutAttribute.Right, 1f, -ItemHPadding),
				NSLayoutConstraint.Create (_baseRateTitleLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _labels [0], NSLayoutAttribute.Top, 1f, -4f)
			});

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

		private VehicleType _vehicleType;
		public VehicleType VehicleType 
		{ 
			get
			{ 
				return _vehicleType;	
			}

			set
			{ 
				_vehicleType = value;

				if (value != null) 
				{
					Initialize ();
				}
			}
		}

		public BaseRateInfo BaseRate
		{
			get
			{ 
				return VehicleType != null ? VehicleType.BaseRate : null;
			}
		}

		public ServiceType ServiceType
		{
			get
			{ 
				return VehicleType != null ? VehicleType.ServiceType : null;
			}
		}
    }
}
