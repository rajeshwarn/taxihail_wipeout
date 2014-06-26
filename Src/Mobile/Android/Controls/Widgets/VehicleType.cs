using System;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Graphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class VehicleType : LinearLayout
	{
		private ImageView VehicleTypeImage { get; set; }
		private TextView VehicleTypeLabel { get; set; }
		private bool IsForSelection { get; set; }

		public VehicleType(Context c, IAttributeSet attr) : base(c, attr)
		{
		}

		public VehicleType(Context c, apcurium.MK.Booking.Api.Contract.Resources.VehicleType vehicle, bool isSelected) : base(c)
		{
			Initialize ();

			IsForSelection = true;
			Vehicle = vehicle;
			IsSelected = IsSelected;
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

		private apcurium.MK.Booking.Api.Contract.Resources.VehicleType _vehicle;
		public apcurium.MK.Booking.Api.Contract.Resources.VehicleType Vehicle
		{
			get { return _vehicle; }
			set
			{
				if (_vehicle != value)
				{
					_vehicle = value;

					var image = DrawableHelper.GetDrawableFromString(Resources, string.Format("{0}_badge_selected", value.LogoName.ToLower()));
					VehicleTypeImage.SetImageDrawable(image);
					VehicleTypeImage.SetColorFilter(GetColorFilter(DefaultColorForTextAndImage));
					VehicleTypeLabel.Text = TinyIoCContainer.Current.Resolve<ILocalization>()[value.Name].ToUpper();
				}
			}
		}

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set 
			{
				if (_isSelected != value) 
				{
					_isSelected = value;

					if (_isSelected) 
					{
						VehicleTypeImage.SetColorFilter(GetColorFilter(Resources.GetColor(Resource.Color.company_color)));
					} 
					else 
					{
						VehicleTypeImage.SetColorFilter(GetColorFilter(DefaultColorForTextAndImage));
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
	}
}

