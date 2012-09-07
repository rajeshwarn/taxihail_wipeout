using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using apcurium.Framework.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RideSettingsModel
    {
        private IEnumerable<ListItem> _companyList;
        private IEnumerable<ListItem> _vehicleTypeList;
        private IEnumerable<ListItem> _chargeTypeList;

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
            private set;
        }

        //TODO:Fix this
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

        public string NbOfPassenger
        {
            get { return Data.Passengers.ToString(); }
            set
            {
                int nbPassengers;
                if (int.TryParse(value, out nbPassengers))
                {
                    Data.Passengers = nbPassengers;
                }
            }
        }

        public int VehicleTypeId
        {
            get { return Data.VehicleTypeId; }
            set
            {
                Data.VehicleTypeId = value;
                if (VehicleTypeList != null)
                {
                    var vehicleType = VehicleTypeList.SingleOrDefault(vt => vt.Id == VehicleTypeId);
                    //TODO:Fix this
                    //Data.VehicleTypeName = vehicleType.Display;
                }
            }
        }

        
        //TODO:Fix this
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
                if (CompanyList != null)
                {
                    var company = CompanyList.SingleOrDefault(c => c.Id == ProviderId);
                    //Data.CompanyName = company.Display;
                }
            }
        }

        public string ProviderName
        {
            get { return _companyList.SingleOrDefault(v => v.Id == ProviderId).SelectOrDefault(v => v.Display, ""); }            
        }

        public int ChargeTypeId
        {
            get { return Data.ChargeTypeId; }
            set
            {
                Data.ChargeTypeId = value;
                if (ChargeTypeList != null)
                {
                    var chargeType = ChargeTypeList.SingleOrDefault(ct => ct.Id == ChargeTypeId);
                    //TODO:Fix this
                    //Data.ChargeTypeName = chargeType.Display;
                }
            }
        }

        //TODO:Fix this
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

