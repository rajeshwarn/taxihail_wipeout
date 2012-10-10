using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Android.Graphics;
using Android.Text;

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
    public class TaxiOverlay : ItemizedOverlay
    {
        private OverlayItem _item;
        private string _title;
        private string _subTitle;
        private MapView _owner;

        private int _markerHeight;

        public TaxiOverlay(MapView owner, Drawable taxiImage, string title,string subTitle, GeoPoint point)
            : base(taxiImage)
        {
            _owner = owner;
            _item = new OverlayItem(point, title, null);
            _title = title;
            _subTitle = subTitle;
            BoundCenterBottom(taxiImage);
            
            _markerHeight = taxiImage.Bounds.Height();

            Populate();


        }

        public override void Draw(Android.Graphics.Canvas canvas, MapView mapView, bool shadow)
        {

            if (shadow)
            {
                return;
            }

            base.Draw(canvas, mapView, shadow);



            foreach (var overlay in _owner.Overlays)
            {
                if (overlay is TaxiOverlay)
                {
                    var item = ((TaxiOverlay)overlay)._item;

                    

                    GeoPoint point = item.Point;

                    Point markerBottomCenterCoords = new Point();
                    _owner.Projection.ToPixels(point, markerBottomCenterCoords);

                    /* Find the width and height of the title*/
                    TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
                    Paint paintRect = new Paint();

                    Rect rect = new Rect();
                    paintText.TextSize = 22;
                    paintText.GetTextBounds(_title, 0, item.Title.Length, rect);

                    rect.Inset(-3, -3);
                    rect.OffsetTo(markerBottomCenterCoords.X - rect.Width() / 2, markerBottomCenterCoords.Y - _markerHeight + 8);

                    paintText.TextAlign = Paint.Align.Center;
                    paintText.TextSize = 22;
                    paintText.SetARGB(255, 255, 255, 255);
                    paintRect.SetARGB(0, 0, 0, 0);
                    
                    paintText.SetTypeface( AppFonts.Bold );
                    canvas.DrawRoundRect(new RectF(rect), 2, 2, paintRect);
                    canvas.DrawText(_title, rect.Left + rect.Width() / 2, rect.Bottom - 3, paintText);

                    canvas.DrawText(_subTitle, rect.Left + rect.Width() / 2, rect.Bottom - 3  + rect.Height(), paintText);

                }
            }
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

            RemoveAllBalloons(_owner);

            if (_noteBaloon == null)
            {
                var layoutInflater = (LayoutInflater)Application.Context.GetSystemService(Context.LayoutInflaterService);
                _noteBaloon = (BaloonLayout)layoutInflater.Inflate(Resource.Layout.Control_Baloon, null);
                var layoutParams = new RelativeLayout.LayoutParams(200, 100);
                layoutParams.AddRule(LayoutRules.CenterVertical);
                layoutParams.AddRule(LayoutRules.CenterHorizontal);
                _noteBaloon.LayoutParameters = layoutParams;
            }

            _owner.RemoveView(_noteBaloon);
            _noteBaloon.Visibility = ViewStates.Visible;
            _noteBaloon.FindViewById<TextView>(Resource.Id.note_text).Text = Title;
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


//public void addOverlay(int latitude, int longitude, String title,
//        String snippet)
//{
//    OverlayItem item;

//    GeoPoint geopoint = new GeoPoint(latitude, longitude);
//    item = new OverlayItem(geopoint, title, snippet);
//    mOverlays.add(item);
//    populate();

//}

//public void addOverlay(OverlayItem overlayItem)
//{
//    mOverlays.add(overlayItem);
//    populate();
//}

//private int markerHeight;
//private ArrayList<OverlayItem> mOverlays = new ArrayList<OverlayItem>();

//private static final int FONT_SIZE = 12;
//private static final int TITLE_MARGIN = 3;