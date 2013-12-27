using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Models
{
    public class RideSettingsModel
    {
        private readonly IEnumerable<ListItem> _companyList;
        private readonly IEnumerable<ListItem> _vehicleTypeList;
        private readonly IEnumerable<ListItem> _chargeTypeList;

        public RideSettingsModel(BookingSettings data, IEnumerable<ListItem> companyList, IEnumerable<ListItem> vehicleTypeList, IEnumerable<ListItem> chargeTypeList)
        {
            _companyList = companyList;
            _vehicleTypeList = vehicleTypeList;
            _chargeTypeList = chargeTypeList;
            Data = data;
        }

        public BookingSettings Data
        {
            get;
            set;
        }

        public string Name
        {
            get { return Data.Name; }
            set { Data.Name = value; }
        }

        public string Phone
        {
            get { return Data.Phone; }
            set { Data.Phone = value; }
        }

        public int? VehicleTypeId
        {
            get { return Data.VehicleTypeId; }
            set
            {
                Data.VehicleTypeId = value;
            }
        }

        
        public string VehicleTypeName
        {
            get { return _vehicleTypeList.SingleOrDefault(v => v.Id == VehicleTypeId).SelectOrDefault(v => v.Display, ""); }            
        }

		public int? ProviderId
        {
            get { return Data.ProviderId; }
            set
            {
                Data.ProviderId = value;
            }
        }

        public string ProviderName
        {
            get { return _companyList.SingleOrDefault(v => v.Id == ProviderId).SelectOrDefault(v => v.Display, ""); }            
        }

        public int? ChargeTypeId
        {
            get { return Data.ChargeTypeId; }
            set
            {
                Data.ChargeTypeId = value;
            }
        }

        public string ChargeTypeName
        {
            get { return _chargeTypeList.SingleOrDefault(v => v.Id == ChargeTypeId).SelectOrDefault(v => v.Display, ""); }            
        }
            
            
           public ListItem[] CompanyList { get { return _companyList.ToArray(); } }
        public ListItem[] VehicleTypeList { get { return _vehicleTypeList.ToArray(); } }
        public ListItem[] ChargeTypeList { get { return _chargeTypeList.ToArray(); } }

        public List<string> ToStringList(ListItem[] listItem)
        {
            return listItem.Select(l => l.Display).ToList();
        }
    }
}

