using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class ListItemAdapter : ArrayAdapter<ListItemData>
    {
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
            //we use the same layout as the GetView so we call it, see adapter ctor call to change the layout
            return GetView(position, convertView, parent);
        }


        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var v = convertView;

            if (v == null)
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
                    listImage.SetImageResource(int.Parse(p.Image));
                }
            }
            return v;
        }
    }
}