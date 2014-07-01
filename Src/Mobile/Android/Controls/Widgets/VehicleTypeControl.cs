using System;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Graphics;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class VehicleTypeControl : LinearLayout
	{
		private ImageView VehicleTypeImage { get; set; }
		private TextView VehicleTypeLabel { get; set; }
		private bool IsForSelection { get; set; }

		public VehicleTypeControl(Context c, IAttributeSet attr) : base(c, attr)
		{
		}

		public VehicleTypeControl(Context c, VehicleType vehicle, bool isSelected) : base(c)
		{
			Initialize ();

			IsForSelection = true;
			Vehicle = vehicle;
			Selected = isSelected;
			Clickable = true;
		}

		protected override void OnFinishInflate()
		{
			base.OnFinishInflate();

			Initialize ();
		}

		private void Initialize()
		{
			var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
			var layout = inflater.Inflate(Resource.Layout.Control_VehicleType, this, true);

			VehicleTypeImage = (ImageView)layout.FindViewById(Resource.Id.vehicleTypeImage);
			VehicleTypeLabel = (TextView)layout.FindViewById(Resource.Id.vehicleTypeLabel);
		}

		private VehicleType _vehicle;
		public VehicleType Vehicle
		{
			get { return _vehicle; }
			set
			{
				if (_vehicle != value)
				{
					_vehicle = value;

					VehicleTypeImage.SetImageDrawable(GetImage(value.LogoName));
					VehicleTypeImage.SetColorFilter(GetColorFilter(DefaultColorForTextAndImage));
					VehicleTypeLabel.Text = TinyIoCContainer.Current.Resolve<ILocalization>()[value.Name].ToUpper();
					VehicleTypeLabel.SetTextColor (DefaultColorForTextAndImage);
				}
			}
		}

		public override bool Selected 
		{
			get { return base.Selected; }
			set 
			{
			    if (base.Selected != value)
			    {
                    VehicleTypeImage.SetImageDrawable(GetImage(Vehicle.LogoName));
			    }

				if (value) 
				{
					VehicleTypeImage.SetColorFilter(GetColorFilter(Resources.GetColor(Resource.Color.company_color)));
					VehicleTypeLabel.SetTextColor (Resources.GetColor(Resource.Color.company_color));
				} 
				else 
				{
					VehicleTypeImage.SetColorFilter(GetColorFilter(DefaultColorForTextAndImage));
					VehicleTypeLabel.SetTextColor (DefaultColorForTextAndImage);
				}

			    base.Selected = value;
			}
		}

		private Color DefaultColorForTextAndImage
		{
			get 
			{ 
				return IsForSelection 
					? Resources.GetColor(Resource.Color.gray)
					: Resources.GetColor(Resource.Color.label_text_color);
			}
		}

		private ColorFilter GetColorFilter(Color color)
		{
			int iColor = color;

			int red = (iColor & 0xFF0000) / 0xFFFF;
			int green = (iColor & 0xFF00) / 0xFF;
			int blue = iColor & 0xFF;

			float[] matrix = { 0, 0, 0, 0, red
				, 0, 0, 0, 0, green
				, 0, 0, 0, 0, blue
				, 0, 0, 0, 1, 0 };

			ColorFilter colorFilter = new ColorMatrixColorFilter(matrix);

			return colorFilter;
		}

        private Drawable GetImage(string vehicleTypeLogoName)
	    {
	        return DrawableHelper.GetDrawableFromString(Resources,
	            string.Format(Selected 
                    ? "{0}_badge_selected" 
                    : "{0}_badge", 
                vehicleTypeLogoName.ToLower()));
	    }
	}
}

