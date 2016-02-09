using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class ListItemAdapter : ArrayAdapter<ListItemData>
    {
		private View _spinnerEmptyItemView;
		private const string SpinnerEmptyItemTag = "SpinnerEmptyItem";

        public ListItemAdapter(Context context, int textViewResourceId) : base(context, textViewResourceId)
        {
        }

        public ListItemAdapter(Context context, int resource, List<ListItemData> items) : base(context, resource, items)
        {
        }

        public ListItemAdapter(Context context, int resource, int textViewResourceId, List<ListItemData> items)
            : base(context, resource, textViewResourceId, items)
        {
        }

		public override View GetDropDownView(int position, View convertView, ViewGroup parent)
		{

			var p = GetItem(position);
			var v = convertView;

			// View Selector
			if (p.Key == ListItemData.SpinnerEmptyItemKey) // Not using the tag/convertview logic as this is a 0px view
			{
				var inflater = (LayoutInflater)Context.GetSystemService (Context.LayoutInflaterService);
				v = _spinnerEmptyItemView ?? (_spinnerEmptyItemView = inflater.Inflate (Resource.Layout.SpinnerEmptyItem, null));

			} else
			{
				v = GetView(position, convertView, parent);
			}

			return v; 
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var v = convertView;

			if (v == null || GetTag(v) == SpinnerEmptyItemTag) // For now, no tag lookup as there is just one other view.
			{
				var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
				v = inflater.Inflate(Resource.Layout.SpinnerTextWithImage, null);
			}

			var p = GetItem(position);
			if (p != null)
			{
				var listTitle = (CheckedTextView) v.FindViewById(Resource.Id.labelSpinner);
				var listImage = (ImageView) v.FindViewById(Resource.Id.imageSpinner);

				if (listTitle != null)
				{
					listTitle.Text = p.Value;
				}
				if (listImage != null
					&& p.Image != null)
				{
					var resource = Context.Resources.GetIdentifier(p.Image.ToLower(), "drawable", Context.PackageName);
					if (resource != 0)
					{
						listImage.SetImageResource(resource);
					}
				}
			}
			return v;
		}

		private string GetTag(View v)
		{
			return v.Tag == null ? "" : v.Tag.ToString ();	
		}
    }
}