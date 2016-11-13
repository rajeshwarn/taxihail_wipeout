using Android.Animation;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Android.Views;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.mk.booking.mobile.client.controls.widgets.BaseRateControl")]
	public class BaseRateControl : LinearLayout
    {
		private const int BaseRateControlHeightInDip = 110;

		private List<TextView> _labels = new List<TextView>();
		private List<TextView> _descriptions = new List<TextView>();
		private TextView _baseRateTitleLabel;

        public BaseRateControl(Context c, IAttributeSet attr) : base(c, attr)
        {
        }

		protected override void OnFinishInflate ()
		{
			base.OnFinishInflate ();
			var inflater = (LayoutInflater)Context.GetSystemService (Context.LayoutInflaterService);
			var layout = inflater.Inflate (Resource.Layout.Control_BaseRate, this, true);

			_labels.Clear();
			_descriptions.Clear();

			_baseRateTitleLabel = FindViewById<TextView>(Resource.Id.baseRateTitle);
			_baseRateTitleLabel.Text = Localize("BaseRate_RateStructure");

			var labelTexts = new [] { 
				Localize ("BaseRate_MinimumFare"),  
				Localize ("BaseRate_BaseRate"), 
				Localize ("BaseRate_PerMileRate"), 
				Localize ("BaseRate_WaitTime")
			};

			var labelIds = new [] { 
				Resource.Id.baseRateLabel1, 
				Resource.Id.baseRateLabel2, 
				Resource.Id.baseRateLabel3, 
				Resource.Id.baseRateLabel4
			};

			var descriptionIds = new [] { 
				Resource.Id.baseRateDescription1, 
				Resource.Id.baseRateDescription2, 
				Resource.Id.baseRateDescription3, 
				Resource.Id.baseRateDescription4
			};

			for (int i = 0; i < labelIds.Length; i++)
			{
				var label = layout.FindViewById<TextView>(labelIds[i]);
				label.Text = labelTexts[i];
				_labels.Add(label);
				var description = layout.FindViewById<TextView>(descriptionIds[i]);
				_descriptions.Add(description);
			}
        }

		public bool BaseRateToggled { get; set; }	

		string ToCurrency(decimal amount)
        {
			return CultureProvider.FormatCurrency((float) amount);
        }

        string Localize(string key) {
        	return this.Services().Localize[key];
        }

        public void ToggleBaseRate ()
		{
			BaseRateToggled = !BaseRateToggled;

            var contentHeightPixels = (int) (BaseRateControlHeightInDip * Context.Resources.DisplayMetrics.Density);
            var animation = BaseRateToggled 
                ? ValueAnimator.OfInt(0, contentHeightPixels) 
                : ValueAnimator.OfInt(contentHeightPixels, 0);

            animation.SetDuration(500);
            animation.Update += (sender, e) => {
                var value = (int) animation.AnimatedValue;
                LayoutParameters.Height = value;
                RequestLayout();
            };

            animation.Start();
        }

		private void Initialize()
		{
			//var mileageRateText = ServiceType == ServiceType.Taxi ? Localize ("BaseRate_PerQuarterMile") : Localize ("BaseRate_PerTenthMile");
			//var mileageRateAmount = ServiceType == ServiceType.Taxi ? ToCurrency (BaseRate.PerMileRate / 4): ToCurrency (BaseRate.PerMileRate / 10);
            var mileageRateText = ServiceType == ServiceType.Taxi ? Localize("BaseRate_PerQuarterMile") : Localize("BaseRate_PerSixthMile");
            var mileageRateAmount = ServiceType == ServiceType.Taxi ? ToCurrency(BaseRate != null ? BaseRate.PerMileRate : 0 / 4) : ToCurrency(BaseRate != null ? BaseRate.PerMileRate : 0 / 6);

            var waitTimeText = ServiceType == ServiceType.Taxi ? Localize ("BaseRate_PerEightySeconds") : Localize ("BaseRate_PerMinute");
			var waitTimeAmount = ServiceType == ServiceType.Taxi ? ToCurrency(BaseRate != null ? BaseRate.WaitTime: 0 * 1.3333333333m) : ToCurrency (BaseRate != null ? BaseRate.WaitTime : 0);

			var descriptionsText = BaseRate != null 
				? new [] { 
				ToCurrency (BaseRate.MinimumFare),
				ToCurrency (BaseRate.BaseRateNoMiles), 
				string.Format (mileageRateText, ToCurrency (BaseRate.PerMileRate), mileageRateAmount),
				string.Format (waitTimeText, waitTimeAmount)
			} : new string[4];

			var revisedNumberOfItems = ServiceType == ServiceType.Taxi ? descriptionsText.Length : descriptionsText.Length - 1;

			for (int i = 0; i < descriptionsText.Length; i++)
			{
				_descriptions [i].Text = descriptionsText [i];
				if (i >= revisedNumberOfItems)
				{
					_descriptions[i].Text = String.Empty;
					_labels[i].Visibility = ViewStates.Invisible;
				}
				else
				{
					_labels[i].Visibility = ViewStates.Visible;
				}
			}

			if (BaseRate == null)
			{
				BaseRateToggled = false;
				LayoutParameters.Height = 0;
				RequestLayout();
			}
		}

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
				return VehicleType != null ? VehicleType.ServiceType : default(ServiceType);
			}
		}
    }
}

