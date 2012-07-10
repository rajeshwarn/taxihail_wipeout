//using System;
//using System.Collections.Generic;
//using System.Text;
//using apcurium.Framework.Extensions;
//using Android.OS;
//using TaxiMobileApp;
//namespace apcurium.MK.Booking.Mobile.Client.Models
//{
// public class LocationModel  :IParcelable
//    {

//     public LocationData Data { get; set; }    
//        public int DescribeContents()
//        {
//            return 0;
//        }

//        public void WriteToParcel(Parcel dest, int flags)
//        {
//            dest.WriteInt(Data.Id);
//            dest.WriteString(Data.Address);
//            dest.WriteString(Data.Apartment);
//            dest.WriteString(Data.Name);
//            dest.WriteString(Data.RingCode);
//            dest.WriteDouble(Data.Latitude.HasValue ? Data.Latitude.Value : 0);
//            dest.WriteDouble(Data.Longitude.HasValue ? Data.Longitude.Value : 0);         
//        }

//        public IntPtr Handle
//        {
//            get { return new IntPtr(); }
//        }
//    }
//}
