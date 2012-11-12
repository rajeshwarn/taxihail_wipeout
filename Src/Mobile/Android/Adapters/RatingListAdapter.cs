using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Models;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class RatingListAdapter : BaseAdapter<RatingModel>
    {
        private readonly Activity _context;
        public List<RatingModel> ListRating { get; set; }
       

        public RatingListAdapter(Activity context, List<RatingModel> objects)
            : base()
        {
            ListRating = objects;
            this._context = context;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = ListRating[position];

            View view=null;
            TitleSubTitleListItemController controller = null;
            if (convertView == null)
            {
                view = _context.LayoutInflater.Inflate(Resource.Layout.RatingListItem, null);
            }
            else
            {
                view =convertView;
            }
            view.FindViewById<TextView>(Resource.Id.RatingTypeNameId).Text = item.RatingTypeName;
             var _rating1 = view.FindViewById<Button>(Resource.Id.RatingScore1);
             var _rating2 = view.FindViewById<Button>(Resource.Id.RatingScore2);
             var _rating3 = view.FindViewById<Button>(Resource.Id.RatingScore3);
             var _rating4 = view.FindViewById<Button>(Resource.Id.RatingScore4);
             var _rating5 = view.FindViewById<Button>(Resource.Id.RatingScore5);
             _rating1.Click += (sender, args) =>
             {
                 item.Score = 1;
                                                var button =
                                                    (Button) sender;
                                                _rating1.Selected = false;
                                                _rating2.Selected = false;
                                                _rating3.Selected = false;
                                                _rating4.Selected = false;
                                                _rating5.Selected = false;
                button.Selected = true;
            };

             _rating2.Click += (sender, args) =>
            {
                item.Score = 2; var button =
                      (Button)sender;
                _rating1.Selected = false;
                _rating2.Selected = false;
                _rating3.Selected = false;
                _rating4.Selected = false;
                _rating5.Selected = false;
                button.Selected = true;
            };

             _rating3.Click += (sender, args) =>
            {
                item.Score = 3; var button =
                       (Button)sender;
                _rating1.Selected = false;
                _rating2.Selected = false;
                _rating3.Selected = false;
                _rating4.Selected = false;
                _rating5.Selected = false;
                button.Selected = true;
            };

             _rating4.Click += (sender, args) =>
            {
                item.Score = 4; var button =
                       (Button)sender;
                _rating1.Selected = false;
                _rating2.Selected = false;
                _rating3.Selected = false;
                _rating4.Selected = false;
                _rating5.Selected = false;
                button.Selected = true;
            };

             _rating5.Click += (sender, args) =>
            {
                item.Score = 5; var button =
                      (Button)sender;
                _rating1.Selected = false;
                _rating2.Selected = false;
                _rating3.Selected = false;
                _rating4.Selected = false;
                _rating5.Selected = false;
                button.Selected = true;
            };

            return view;
        }

        public override int Count
        {
            get { return ListRating.Count; }
        }

        public override RatingModel this[int position]
        {
            get { return ListRating[position]; }
        }
    }
}