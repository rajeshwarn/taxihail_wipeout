#region

using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Database;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Database
{
    [DbConfigurationType(typeof(CustomDbConfiguration))]
    public class BookingDbContext : DbContext
    {
        public const string SchemaName = "Booking";

        public BookingDbContext()
        {
            
        }

        public BookingDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make the name of the views match exactly the name of the corresponding property.
            modelBuilder.ComplexType<Address>(); //doing here because address is shared among several projects, layers
            modelBuilder.ComplexType<DriverInfos>(); // same for drivers infos
            modelBuilder.ComplexType<CountryISOCode>();
            modelBuilder.ComplexType<BookingSettings>();
            modelBuilder.Entity<OrderStatusDetail>()
                .HasKey(x => x.OrderId)
                .ToTable("OrderStatusDetail", SchemaName);
            modelBuilder.Entity<OrderPairingDetail>()
                .HasKey(x => x.OrderId)
                .ToTable("OrderPairingDetail", SchemaName);
            modelBuilder.Entity<OrderManualRideLinqDetail>()
                .HasKey(x => x.OrderId)
                .ToTable("OrderManualRideLinqDetail", SchemaName);

            modelBuilder.Entity<OrderVehiclePositionDetail>().ToTable("OrderVehiclePositionDetail", SchemaName);
            modelBuilder.Entity<PromotionDetail>().ToTable("PromotionDetail", SchemaName);
            modelBuilder.Entity<PromotionUsageDetail>().ToTable("PromotionUsageDetail", SchemaName);
            modelBuilder.Entity<PromotionProgressDetail>().ToTable("PromotionProgressDetail", SchemaName);
            modelBuilder.Entity<AccountDetail>().ToTable("AccountDetail", SchemaName);
            modelBuilder.Entity<AccountNoteDetail>().ToTable("AccountNoteDetail", SchemaName);
            modelBuilder.Entity<AccountIbsDetail>().ToTable("AccountIbsDetail", SchemaName);
            modelBuilder.Entity<DeviceDetail>().ToTable("DeviceDetail", SchemaName);
            modelBuilder.Entity<OrderStatusUpdateDetail>().ToTable("OrderStatusUpdateDetail", SchemaName);
            modelBuilder.Entity<AddressDetails>().ToTable("AddressDetails", SchemaName);
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail", SchemaName);
            modelBuilder.Entity<TariffDetail>().ToTable("TariffDetail", SchemaName);
            modelBuilder.Entity<RuleDetail>().ToTable("RuleDetail", SchemaName);
            modelBuilder.Entity<RatingTypeDetail>().ToTable("RatingTypeDetail", SchemaName);
            modelBuilder.Entity<DefaultAddressDetails>().ToTable("DefaultAddressDetails", SchemaName);
            modelBuilder.Entity<PopularAddressDetails>().ToTable("PopularAddressDetails", SchemaName);
            modelBuilder.Entity<OrderRatingDetails>().ToTable("OrderRatingDetails", SchemaName);
            modelBuilder.Entity<RatingScoreDetails>().ToTable("RatingScoreDetails", SchemaName);
            modelBuilder.Entity<CreditCardDetails>().ToTable("CreditCardDetails", SchemaName);
            modelBuilder.Entity<OrderPaymentDetail>().ToTable("OrderPaymentDetail", SchemaName);
            modelBuilder.Entity<CompanyDetail>().ToTable("CompanyDetail", SchemaName);
            modelBuilder.Entity<OrderUserGpsDetail>().ToTable("OrderUserGpsDetail", SchemaName);
            modelBuilder.Entity<AppStartUpLogDetail>().ToTable("AppStartUpLogDetail", SchemaName);
            modelBuilder.Entity<PayPalAccountDetails>().ToTable("PayPalAccountDetails", SchemaName);
            modelBuilder.Entity<OrderReportDetail>().ToTable("OrderReportDetail", SchemaName);
            modelBuilder.Entity<OrderNotificationDetail>().ToTable("OrderNotificationDetail", SchemaName);
            modelBuilder.Entity<OverduePaymentDetail>().ToTable("OverduePaymentDetail", SchemaName);
            modelBuilder.Entity<FeesDetail>().ToTable("FeesDetail", SchemaName);
            modelBuilder.Entity<BlackListEntry>().ToTable("BlackListEntry", SchemaName);
            modelBuilder.Entity<ConfigurationChangeEntry>().ToTable("ConfigurationChangeEntry", SchemaName);

            modelBuilder.Entity<AccountChargeQuestion>().ToTable("AccountChargeQuestion", SchemaName);
            modelBuilder.Entity<AccountChargeDetail>().ToTable("AccountChargeDetail", SchemaName)
                .HasMany(x => x.Questions)
                .WithRequired()
                .HasForeignKey(x => x.AccountId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<VehicleTypeDetail>().ToTable("VehicleTypeDetail", SchemaName);
            modelBuilder.Entity<TemporaryOrderCreationInfoDetail>().ToTable("TemporaryOrderCreationInfoDetail", SchemaName);
            modelBuilder.Entity<TemporaryOrderPaymentInfoDetail>().ToTable("TemporaryOrderPaymentInfoDetail", SchemaName);
            modelBuilder.Entity<AccountChargeQuestionAnswer>().ToTable("AccountChargeQuestionAnswer", SchemaName);

            modelBuilder.Entity<Airline>().ToTable("Airline", SchemaName);
            modelBuilder.Entity<PickupPoint>().ToTable("PickupPoint", SchemaName);
        }

        public T Find<T>(Guid id) where T : class
        {
            return Set<T>().Find(id);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return Set<T>();
        }

        public void Save<T>(T entity) where T : class
        {
            var entry = Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                Set<T>().Add(entity);
            }

            SaveChanges();
        }

		public static DateTime MinimumDatabaseDateTime
		{
			get
			{
				return new DateTime(System.Data.SqlTypes.SqlDateTime.MinValue.TimeTicks);
			}
		}
    }
}