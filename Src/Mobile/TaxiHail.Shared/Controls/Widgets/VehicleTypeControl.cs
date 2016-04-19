﻿using Android.Graphics.Drawables;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Graphics;
using apcurium.MK.Booking.Api.Contract.Resources;
using Android.Runtime;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.mk.booking.mobile.client.controls.widgets.VehicleTypeControl")]
	public class VehicleTypeControl : LinearLayout
	{
		private ImageView VehicleTypeImage { get; set; }
		private TextView VehicleTypeLabel { get; set; }
		private bool IsForSelection { get; set; }
		private bool EtaBadge { get; set; }

		public VehicleTypeControl(Context c, IAttributeSet attr) : base(c, attr)
		{
		}

		public VehicleTypeControl(Context c, VehicleType vehicle, bool isSelected) : base(c)
		{
			EtaBadge = false;
			Initialize ();
			IsForSelection = true;
			Vehicle = vehicle;
			Selected = isSelected;
			Clickable = true;
		}

		public VehicleTypeControl(Context c, VehicleType vehicle) : base(c)
		{
			EtaBadge = true;
			Selected = true;
			Initialize ();
			IsForSelection = false;
			Vehicle = vehicle;
			Clickable = false;
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

			if (EtaBadge) {
				VehicleTypeLabel.Visibility = ViewStates.Gone;
				VehicleTypeImage.SetPadding (0, 0, 0, 0);
			}
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

					VehicleTypeImage.SetImageDrawable(GetImage(value.LogoName, EtaBadge));
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
                    base.Selected = value;

                    if (Vehicle == null)
                    {
                        return;
                    }

					VehicleTypeImage.SetImageDrawable(GetImage(Vehicle.LogoName, EtaBadge));
			    
					if (value && !EtaBadge) 
                    {
                        var companyColor = Resources.GetColor(Resource.Color.company_color);

                        //If the company color is too light
                        if (DrawHelper.IsThisColorLight(companyColor))
                        {
                            companyColor = Color.Black; //Change the color of the vehicle selector to black
                        }

                        VehicleTypeImage.SetColorFilter(GetColorFilter(companyColor));
                        VehicleTypeLabel.SetTextColor(companyColor);
                    } 
                    else 
                    {
                        VehicleTypeImage.SetColorFilter(GetColorFilter(DefaultColorForTextAndImage));
                        VehicleTypeLabel.SetTextColor (DefaultColorForTextAndImage);
                    }
                }
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

		private Drawable GetImage(string vehicleTypeLogoName, bool etaBadge = false)
	    {
	        return DrawableHelper.GetDrawableFromString(Resources,
	            string.Format(Selected 
					? "{0}_" + (etaBadge ? "no_" : "") + "badge_selected" 
					: "{0}_" + (etaBadge ? "no_" : "") + "badge", 
                vehicleTypeLogoName.ToLower()));
	    }
	}
}

