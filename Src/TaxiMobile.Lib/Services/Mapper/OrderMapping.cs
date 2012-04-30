using System;
using System.Linq;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework.Extensions;


namespace TaxiMobile.Lib.Services.Mapper
{
	public class OrderMapping
	{
		public void UpdateHistory (AccountData existingAccount, TBookOrder_6[] orders, ListItem[] vehicules, ListItem[] companies, ListItem[] chargeTypes)
		{
			orders.ForEach (order =>
			{
				var toUpdate = existingAccount.BookingHistory.FirstOrDefault (b => b.Id == order.OrderID);
				
				if (toUpdate == null) {
										
					toUpdate = existingAccount.AddNewHistory();
				}

                toUpdate.Id = order.OrderID;
				UpdateOrder( toUpdate , order,vehicules,companies, chargeTypes );
			});
			
		}


        public void UpdateOrder(BookingInfoData toUpdate, TBookOrder_6 order, ListItem[] vehicules, ListItem[] companies, ListItem[] chargeTypes)
		{
            if (toUpdate.Settings == null)
            {
                toUpdate.Settings = new BookingSetting();
            }

            toUpdate.Settings.ChargeType = order.ChargeTypeID;

            toUpdate.Settings.Company = order.ServiceProviderID;
            toUpdate.Settings.CompanyName = companies.FirstOrDefault(c => c.Id == order.ServiceProviderID).SelectOrDefault(c => c.Display, "");

            toUpdate.Settings.Phone = order.ContactPhone;
            toUpdate.Settings.Name = order.Customer;
            toUpdate.Settings.Passengers = order.Passengers;
            toUpdate.Settings.VehicleType = order.VehicleTypeID;
            toUpdate.Settings.VehicleTypeName = vehicules.FirstOrDefault(c => c.Id == order.VehicleTypeID).SelectOrDefault(c => c.Display, "");

            toUpdate.Settings.ChargeType = chargeTypes.FirstOrDefault(c => c.Id == order.ChargeTypeID).SelectOrDefault(c => c.Id, chargeTypes[0].Id);
            toUpdate.Settings.ChargeTypeName = chargeTypes.FirstOrDefault(c => c.Id == toUpdate.Settings.ChargeType).SelectOrDefault(c => c.Display, "");

            toUpdate.Status = order.OrderStatus.ToString();

            //toUpdate.PickupLocation.RingCode = order.RingCode;

            toUpdate.RequestedDateTime = order.OrderDate.ToDateTime();


            toUpdate.PickupDate = order.PickupTime.ToDateTime();
						
		}

        public TBookOrder_6 ToWSOrder(BookingInfoData info)
		{
            var order = new TBookOrder_6();
            order.ChargeTypeID = info.Settings.ChargeType;
            order.ServiceProviderID = info.Settings.Company;
            order.ContactPhone = info.Settings.Phone;
            order.Customer = info.Settings.Name;
            order.Passengers = info.Settings.Passengers;
            order.VehicleTypeID = info.Settings.VehicleType;

            if (info.RequestedDateTime.HasValue)
            {
                order.OrderDate = info.RequestedDateTime.Value.ToWSDateTime();
            }
            if (info.PickupLocation.RingCode.HasValue())
            {
                //order.RingCode = info.PickupLocation.RingCode;
            }

            if (info.PickupDate.HasValue)
            {
                order.PickupTime = info.PickupDate.Value.ToWSDateTime();
            }
            else
            {
                order.PickupTime = DateTime.Now.AddMinutes(5).ToWSDateTime();
            }

            if (info.DestinationLocation != null)
            {
                info.DestinationLocation.Address = info.DestinationLocation.Address.SelectOrDefault(a => a, "");

            }
			
			return order;
		}
		
	}
}

