//using System;
//using System.Collections.Generic;
//using Android.App;
//using Android.Graphics;
//using Android.Views;
//using Android.Widget;
//using apcurium.MK.Booking.Mobile.Client.Models;

//namespace apcurium.MK.Booking.Mobile.Client.Adapters
//{
//    public class IconActionAdapter : BaseAdapter<IconAction>
//    {
//        private Activity context;
//        private List<IconAction> listIconAction;
//        public IconActionAdapter(Activity context, int resource, List<IconAction> objects)
//            : base()
//        {
//            listIconAction = objects;
//            this.context = context;
//        }

//        public override long GetItemId(int position)
//        {
//            return position;
//        }

//        public override View GetView(int position, View convertView, ViewGroup parent)
//        {
//            var item = listIconAction[position];
//            View view = convertView;
//            if (view == null)
//            {
//                view = context.LayoutInflater.Inflate(Resource.Layout.dropdownRow, null);
//            }
//            var icon = view.FindViewById<ImageView>(Resource.Id.icon);
//            try
//            {
//                Bitmap bitmap = BitmapFactory.DecodeStream(this.context.Resources.Assets.Open(item.ImageUri));
//                icon.SetImageBitmap(bitmap);
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//            return view;
//        }

//        public override int Count
//        {
//            get { return listIconAction.Count; }
//        }

//        public override IconAction this[int position]
//        {
//            get { return listIconAction[position]; }
//        }
//    }
//}