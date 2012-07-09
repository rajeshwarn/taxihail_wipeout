using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using apcurium.Framework.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RideSettingsModel
    {
        private IEnumerable<ListItem> _companyList;
        private IEnumerable<ListItem> _vehicleTypeList;
        private IEnumerable<ListItem> _chargeTypeList;

        public RideSettingsModel(BookingSetting data, IEnumerable<ListItem> companyList, IEnumerable<ListItem> vehicleTypeList, IEnumerable<ListItem> chargeTypeList)
        {
            _companyList = companyList;
            _vehicleTypeList = vehicleTypeList;
            _chargeTypeList = chargeTypeList;
            Data = data;
        }

        public BookingSetting Data
        {
            get;
            private set;
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

        public int VehicleType
        {
            get { return Data.VehicleType; }
            set
            {
                Data.VehicleType = value;
                if (VehicleTypeList != null)
                {
                    var vehicleType = VehicleTypeList.SingleOrDefault(vt => vt.Id == VehicleType);
                    Data.VehicleTypeName = vehicleType.Display;
                }
            }
        }

        public string VehicleTypeName
        {
            get { return Data.VehicleTypeName; }
            private set { Data.VehicleTypeName = value; }
        }


        public int Company
        {
            get { return Data.Company; }
            set
            {
                Data.Company = value;
                if (CompanyList != null)
                {
                    var company = CompanyList.SingleOrDefault(c => c.Id == Company);
                    Data.CompanyName = company.Display;
                }
            }
        }

        public string CompanyName
        {
            get { return Data.CompanyName; }
            private set { Data.CompanyName = value; }
        }

        public int ChargeType
        {
            get { return Data.ChargeType; }
            set
            {
                Data.ChargeType = value;
                if (ChargeTypeList != null)
                {
                    var chargeType = ChargeTypeList.SingleOrDefault(ct => ct.Id == ChargeType);
                    Data.ChargeTypeName = chargeType.Display;
                }
            }
        }
        public string ChargeTypeName
        {
            get { return Data.ChargeTypeName; }
            private set { Data.ChargeTypeName = value; }
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

