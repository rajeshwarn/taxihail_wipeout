using apcurium.MK.Common.Enumeration;
using System;

namespace apcurium.MK.Common.Entity
{
	public class EditableVehicle : ListItem
	{
        public ServiceType ServiceType { get; set; }
        public int IbsVehicleId { get; set; }
	}
}