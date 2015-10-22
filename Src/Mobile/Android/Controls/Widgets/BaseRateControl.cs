using Android.Animation;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Android.Views;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("apcurium.mk.booking.mobile.client.controls.widgets.BaseRateControl")]
	public class BaseRateControl : LinearLayout
    {
		private const int BaseRateControlHeightInDip = 106;

		private List<TextView> _labels = new List<TextView>();
		private List<TextView> _descriptions = new List<TextView>();

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

			var labelTexts = new [] { 
				Localize ("BaseRate_MinimumFare"),  
				Localize ("BaseRate_BaseRate"), 
				Localize ("BaseRate_PerMileRate"), 
				Localize ("BaseRate_WaitTime"), 
				Localize ("BaseRate_AirportMeetAndGreet")
			};

			var labelIds = new [] { 
				Resource.Id.baseRateLabel1, 
				Resource.Id.baseRateLabel2, 
				Resource.Id.baseRateLabel3, 
				Resource.Id.baseRateLabel4, 
				Resource.Id.baseRateLabel5
			};

			var descriptionIds = new [] { 
				Resource.Id.baseRateDescription1, 
				Resource.Id.baseRateDescription2, 
				Resource.Id.baseRateDescription3, 
				Resource.Id.baseRateDescription4, 
				Resource.Id.baseRateDescription5
			};

			for (int i = 0; i < labelIds.Length; i++)
			{
				var label = layout.FindViewById<TextView>(labelIds[i]);
				label.Text = labelTexts[i];
				var description = layout.FindViewById<TextView>(descriptionIds[i]);
				_descriptions.Add(description);
			}
        }

		public bool BaseRateToggled { get; set; }	

        private BaseRateInfo _baseRate;
        public BaseRateInfo BaseRate
		{
            get { return _baseRate; }
			set
			{
                _baseRate = value;
				var descriptionsText = value != null 
                    ? new [] { 
    					ToCurrency (value.MinimumFare),
    					ToCurrency (value.BaseRateNoMiles), 
    					string.Format (Localize ("BaseRate_PerTenthMile"), ToCurrency (value.PerMileRate), ToCurrency (value.PerMileRate / 10)),
    					string.Format (Localize ("BaseRate_PerMinute"), ToCurrency (value.WaitTime)),
    					ToCurrency (value.AirportMeetAndGreet)
				    } 
                    : new string[5];

				for (int i = 0; i < descriptionsText.Length; i++)
				{
					_descriptions [i].Text = descriptionsText [i];
				}
					
				if (value == null)
				{
					BaseRateToggled = false;
					LayoutParameters.Height = 0;
        			RequestLayout();
				}
        	}
        }

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
    }
}

