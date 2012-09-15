//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Android.GoogleMaps;

//namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
//{
//    public class AddressController
//    {

//        private bool _showUserLocation;

//        private MyLocationOverlay _myLocationOverlay;
//        private MapView _map;
//        private BookActivity _parent;
//        public AddressController( BookActivity parent, MapView map, bool showUserLocation)
//        {
//            _map = map;
//            _parent = parent;
//            _showUserLocation = showUserLocation;
//        }




//        private void InitMap(MapView map)
//        {
//            map.SetBuiltInZoomControls(true);
//            map.Clickable = true;
//            map.Traffic = false;
//            map.Satellite = false;
//            map.Controller.SetZoom(18);
//        }


//        void AddMyLocationOverlay(MapView map)
//        {         
//            _myLocationOverlay = new MyLocationOverlay(_parent, map);
//            _myLocationOverlay.EnableMyLocation();
//            map.Overlays.Add(_myLocationOverlay);

//            //_myLocationOverlay.RunOnFirstFix(() =>
//            //{
//            //    if (_myLocationOverlay.LastFix != null)
//            //    {
//            //        map.Controller.AnimateTo(_myLocationOverlay.MyLocation);
//            //    }
//            //});


//        }


//    }
//}