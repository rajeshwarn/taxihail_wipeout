using System;
using System.Linq;
using System.Collections.Generic;

using apcurium.Framework;
using apcurium.Framework.Extensions;
using IBS =TaxiMobileApp.Lib.IBS;
namespace TaxiMobileApp
{
	public class OrderMapping
	{
		public OrderMapping ()
		{
		}

		public void UpdateHistory (AccountData existingAccount, IBS.OrderInfo[] orders, ListItem[] vehicules, ListItem[] companies, ListItem[] chargeTypes)
		{
			orders.ForEach (order =>
			{
				var toUpdate = existingAccount.BookingHistory.FirstOrDefault (b => b.Id == order.Id);
				
				if (toUpdate == null) {
										
					toUpdate = existingAccount.AddNewHistory();
				}
				
				toUpdate.Id = order.Id;
				UpdateOrder( toUpdate , order,vehicules,companies, chargeTypes );
			});
			
		}


		public void UpdateOrder (BookingInfoData toUpdate, IBS.OrderInfo order, ListItem[] vehicules, ListItem[] companies, ListItem[] chargeTypes)
		{
			if ( toUpdate.Settings == null )
			{
				toUpdate.Settings = new BookingSetting();
			}
			
			toUpdate.Settings.ChargeType = order.ChargeTypeId;
			
			toUpdate.Settings.Company = order.CompanyId;
			toUpdate.Settings.CompanyName = companies.FirstOrDefault( c=>c.Id == order.CompanyId ).SelectOrDefault( c=>c.Display, "" );
			
			toUpdate.Settings.Phone = order.ContactPhone;
			toUpdate.Settings.Name = order.Name;
			toUpdate.Settings.Passengers = order.NumberOfPassenger;
			toUpdate.Settings.VehicleType = order.VehicleTypeId;
			toUpdate.Settings.VehicleTypeName = vehicules.FirstOrDefault( c=>c.Id == order.VehicleTypeId ).SelectOrDefault( c=>c.Display, "" );
			
			toUpdate.Settings.ChargeType = chargeTypes.FirstOrDefault( c=>c.Id == order.ChargeTypeId ).SelectOrDefault( c=>c.Id, chargeTypes[0].Id );
			toUpdate.Settings.ChargeTypeName = chargeTypes.FirstOrDefault( c=>c.Id == toUpdate.Settings.ChargeType ).SelectOrDefault( c=>c.Display, "" );
			
			toUpdate.Status ="";
			if ( order.Status !=  null )
			{
				toUpdate.Status = order.Status.Description;
			}
			toUpdate.PickupLocation.RingCode = order.RingCode;
			
			toUpdate.RequestedDateTime = order.OrderDate;
			new AccountMapping ().UpdateLocationData( toUpdate.PickupLocation, order.PickupAddress );
			
			toUpdate.PickupDate = order.PickupTime;
			
			
			if ( (order.DropoffAddress != null) && order.DropoffAddress.AddressAndCity.HasValue() )
			{
				new AccountMapping ().UpdateLocationData( toUpdate.DestinationLocation, order.DropoffAddress );
			}
			
			
						
		}
		
		public IBS.OrderInfo ToWSOrder (BookingInfoData info)
		{
			var order = new IBS.OrderInfo ();
			order.ChargeTypeId = info.Settings.ChargeType;
			order.CompanyId = info.Settings.Company;
			order.ContactPhone = info.Settings.Phone;
			order.Name = info.Settings.Name;
			order.NumberOfPassenger = info.Settings.Passengers;
			order.VehicleTypeId = info.Settings.VehicleType;
			
			if ( info.RequestedDateTime.HasValue )
			{
				order.OrderDate = info.RequestedDateTime.Value;
			}
			if (info.PickupLocation.RingCode.HasValue ()) {
				order.RingCode = info.PickupLocation.RingCode;
			}
						
			order.PickupAddress = new AccountMapping ().ToWSLocationData (info.PickupLocation);
			
			
			if (info.PickupDate.HasValue) {
				order.PickupTime = info.PickupDate.Value;
			} else {
				order.PickupTime = DateTime.Now.AddMinutes (5);
			}
			
			if (info.DestinationLocation != null) {
				info.DestinationLocation.Address = info.DestinationLocation.Address.SelectOrDefault (a => a, "");
				order.DropoffAddress = new AccountMapping ().ToWSLocationData (info.DestinationLocation);
			}
			
			return order;
		}
		
	}
}

