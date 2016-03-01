namespace apcurium.MK.Booking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Booking_MTA_2016_03_01 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Booking.AccountNoteEntry",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AccountId = c.Guid(nullable: false),
                        WriterAccountId = c.Guid(nullable: false),
                        WriterAccountEmail = c.String(),
                        Note = c.String(),
                        Type = c.Int(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "Booking.BlackListEntry",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PhoneNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "Booking.ConfigurationChangeEntry",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AccountId = c.Guid(nullable: false),
                        AccountEmail = c.String(),
                        Date = c.DateTime(nullable: false),
                        OldValues = c.String(),
                        NewValues = c.String(),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("Booking.OrderStatusDetail", "ChargeAmountsTimeOut", c => c.DateTime());
            AddColumn("Booking.OrderStatusDetail", "LastTripPollingDateInUtc", c => c.DateTime());
            AddColumn("Booking.OrderManualRideLinqDetail", "LastLatitudeOfVehicle", c => c.Double());
            AddColumn("Booking.OrderManualRideLinqDetail", "LastLongitudeOfVehicle", c => c.Double());
            AddColumn("Booking.OrderManualRideLinqDetail", "IsWaitingForPayment", c => c.Boolean(nullable: false));
            AddColumn("Booking.AccountDetail", "BraintreeAccountId", c => c.String());
            AddColumn("Booking.OrderStatusUpdateDetail", "CycleStartDate", c => c.DateTime());
            AddColumn("Booking.OrderDetail", "OriginatingIpAddress", c => c.String());
            AddColumn("Booking.OrderDetail", "KountSessionId", c => c.String());
            AddColumn("Booking.OrderDetail", "IsRefunded", c => c.Boolean(nullable: false));
            AddColumn("Booking.RuleDetail", "ExcludeCircularZone", c => c.Boolean(nullable: false));
            AddColumn("Booking.RuleDetail", "ExcludedCircularZoneLatitude", c => c.Double(nullable: false));
            AddColumn("Booking.RuleDetail", "ExcludedCircularZoneLongitude", c => c.Double(nullable: false));
            AddColumn("Booking.RuleDetail", "ExcludedCircularZoneRadius", c => c.Int(nullable: false));
            AddColumn("Booking.OrderRatingDetails", "AccountId", c => c.Guid());
            AddColumn("Booking.OrderReportDetail", "Order_OriginatingIpAddress", c => c.String());
            AddColumn("Booking.OrderReportDetail", "Order_KountSessionId", c => c.String());
            AddColumn("Booking.OrderReportDetail", "OrderStatus_OrderIsNoShow", c => c.Boolean(nullable: false));
            AddColumn("Booking.OrderReportDetail", "Payment_DriverId", c => c.String());
            AddColumn("Booking.OrderReportDetail", "Payment_Medallion", c => c.String());
            AddColumn("Booking.OrderReportDetail", "Payment_Last4Digits", c => c.String());
            AddColumn("Booking.OrderReportDetail", "VehicleInfos_DriverId", c => c.String());
            DropColumn("Booking.RatingScoreDetails", "AccountId");
        }
        
        public override void Down()
        {
            AddColumn("Booking.RatingScoreDetails", "AccountId", c => c.Guid(nullable: false));
            DropColumn("Booking.OrderReportDetail", "VehicleInfos_DriverId");
            DropColumn("Booking.OrderReportDetail", "Payment_Last4Digits");
            DropColumn("Booking.OrderReportDetail", "Payment_Medallion");
            DropColumn("Booking.OrderReportDetail", "Payment_DriverId");
            DropColumn("Booking.OrderReportDetail", "OrderStatus_OrderIsNoShow");
            DropColumn("Booking.OrderReportDetail", "Order_KountSessionId");
            DropColumn("Booking.OrderReportDetail", "Order_OriginatingIpAddress");
            DropColumn("Booking.OrderRatingDetails", "AccountId");
            DropColumn("Booking.RuleDetail", "ExcludedCircularZoneRadius");
            DropColumn("Booking.RuleDetail", "ExcludedCircularZoneLongitude");
            DropColumn("Booking.RuleDetail", "ExcludedCircularZoneLatitude");
            DropColumn("Booking.RuleDetail", "ExcludeCircularZone");
            DropColumn("Booking.OrderDetail", "IsRefunded");
            DropColumn("Booking.OrderDetail", "KountSessionId");
            DropColumn("Booking.OrderDetail", "OriginatingIpAddress");
            DropColumn("Booking.OrderStatusUpdateDetail", "CycleStartDate");
            DropColumn("Booking.AccountDetail", "BraintreeAccountId");
            DropColumn("Booking.OrderManualRideLinqDetail", "IsWaitingForPayment");
            DropColumn("Booking.OrderManualRideLinqDetail", "LastLongitudeOfVehicle");
            DropColumn("Booking.OrderManualRideLinqDetail", "LastLatitudeOfVehicle");
            DropColumn("Booking.OrderStatusDetail", "LastTripPollingDateInUtc");
            DropColumn("Booking.OrderStatusDetail", "ChargeAmountsTimeOut");
            DropTable("Booking.ConfigurationChangeEntry");
            DropTable("Booking.BlackListEntry");
            DropTable("Booking.AccountNoteEntry");
        }
    }
}
