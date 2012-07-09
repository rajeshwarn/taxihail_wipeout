using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.Graphics.Drawables;
using Android.Views;
using TaxiMobile.Controls;
using Android.Widget;

namespace TaxiMobile.MapUtitilties
{
    public class PushPinOverlay : ItemizedOverlay
    {
        private OverlayItem _item;
        private string _title;
        private MapView _owner;

        public PushPinOverlay(MapView owner, Drawable imagePushPint, string title, GeoPoint point)
            : base(imagePushPint)
        {
            _owner = owner;
            _item = new OverlayItem(point, title, null);
            _title = title;
            BoundCenterBottom(imagePushPint);
            Populate();
        }

        public string Title { get { return _item.Title; } }

        protected override Java.Lang.Object CreateItem(int i)
        {
            return _item;
        }

        public override int Size()
        {
            return 1;
        }

        private BaloonLayout _noteBaloon;

        protected override bool OnTap(int index)
        {
            var result = base.OnTap(index);

            var overlay = _owner.Overlays.ElementAt(index); // as PushPinOverlay;
            //if (overlay != null)
            //{

            RemoveAllBalloons(_owner);

            if (_noteBaloon == null)
            {
                var layoutInflater = (LayoutInflater)Application.Context.GetSystemService(Context.LayoutInflaterService);
                _noteBaloon = (BaloonLayout)layoutInflater.Inflate(Resource.Layout.Baloon, null);
                var layoutParams = new RelativeLayout.LayoutParams(200, 100);
                layoutParams.AddRule(LayoutRules.CenterVertical);
                layoutParams.AddRule(LayoutRules.CenterHorizontal);
                _noteBaloon.LayoutParameters = layoutParams;
            }



            _owner.RemoveView(_noteBaloon);
            Console.WriteLine("TAPP");
            _noteBaloon.Visibility = ViewStates.Visible;


            _noteBaloon.FindViewById<TextView>(Resource.Id.note_text).Text = Title;
            //mapController.animateTo(noteOverlay.getTapPoint());
            _owner.AddView(_noteBaloon, new MapView.LayoutParams(200, 100, this.Center, MapView.LayoutParams.BottomCenter));

            return result;
        }


        private void RemoveAllBalloons(MapView mapView)
        {
            if (mapView.Overlays != null)
            {
                foreach (var i in mapView.Overlays.OfType<PushPinOverlay>())
                {
                    i.RemoveBaloon();
                }
            }
        }

        public void RemoveBaloon()
        {
            if (_noteBaloon != null)
            {
                _noteBaloon.Visibility = ViewStates.Gone;
            }
        }
    }
}
