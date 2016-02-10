#region

using System;
using System.Xml.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class OrderCommandHandler :
        ICommandHandler<CreateOrder>,
        ICommandHandler<CancelOrder>,
        ICommandHandler<RemoveOrderFromHistory>,
        ICommandHandler<RateOrder>,
        ICommandHandler<ChangeOrderStatus>,
        ICommandHandler<PairForPayment>,
        ICommandHandler<UnpairForPayment>,
        ICommandHandler<NotifyOrderTimedOut>,
        ICommandHandler<PrepareOrderForNextDispatch>,
        ICommandHandler<InitiateIbsOrderSwitch>,
        ICommandHandler<SwitchOrderToNextDispatchCompany>,
        ICommandHandler<IgnoreDispatchCompanySwitch>,
        ICommandHandler<AddIbsOrderInfoToOrder>,
        ICommandHandler<CancelOrderBecauseOfError>,
        ICommandHandler<SaveTemporaryOrderCreationInfo>,
        ICommandHandler<MarkPrepaidOrderAsSuccessful>,
        ICommandHandler<UpdateRefundedOrder>,
        ICommandHandler<CreateOrderForManualRideLinqPair>,
        ICommandHandler<UnpairOrderForManualRideLinq>,
        ICommandHandler<UpdateTripInfoInOrderForManualRideLinq>,
        ICommandHandler<SaveTemporaryOrderPaymentInfo>,
        ICommandHandler<UpdateAutoTip>,
        ICommandHandler<LogOriginalEta>,
        ICommandHandler<UpdateOrderNotificationDetail>,
		ICommandHandler<CreateReportOrder>,
        ICommandHandler<PayGratuity>,
        ICommandHandler<UpdateOrderInTrip>,
        ICommandHandler<UpdateOrderGratuity>
    {
        private readonly IEventSourcedRepository<Order> _repository;
        private readonly Func<BookingDbContext> _contextFactory;

        public OrderCommandHandler(IEventSourcedRepository<Order> repository, Func<BookingDbContext> contextFactory)
        {
            _repository = repository;
            _contextFactory = contextFactory;
        }
        
        public void Handle(CancelOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.Cancel();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(ChangeOrderStatus command)
        {
            var order = _repository.Find(command.Status.OrderId);
            order.ChangeStatus(command.Status, command.Fare, command.Tip, command.Toll, command.Tax, command.Surcharge);

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(NotifyOrderTimedOut command)
        {
            var order = _repository.Find(command.OrderId);
            order.NotifyOrderTimedOut(command.Market);

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(CreateOrder command)
        {
			var order = _repository.Find(command.OrderId);

			if (order == null)
			{
				order = new Order(command.OrderId);
			}

			order.UpdateOrderCreated(command.AccountId, command.PickupDate,
					command.PickupAddress, command.DropOffAddress, command.Settings, command.EstimatedFare,
					command.UserAgent, command.ClientLanguageCode, command.UserLatitude, command.UserLongitude,
					command.UserNote, command.ClientVersion, command.IsChargeAccountPaymentWithCardOnFile,
                    command.CompanyKey, command.CompanyName, command.Market, command.IsPrepaid, command.BookingFees, command.TipIncentive,
                    command.IbsInformationNote, command.Fare, command.IbsAccountId, command.Prompts, command.PromptsLength,
                    command.PromotionId, command.IsFutureBooking, command.ReferenceDataCompanyList, command.ChargeTypeEmail, command.IbsOrderId,
                    command.OriginatingIpAddress, command.KountSessionId);

            if (command.Payment.PayWithCreditCard)
            {
                var payment = Mapper.Map<PaymentInformation>(command.Payment);
                order.SetPaymentInformation(payment);
            }

            _repository.Save(order, command.Id.ToString());
        }

		public void Handle(CreateReportOrder command)
		{
			var order = _repository.Find(command.OrderId);

			if (order == null)
			{
				order = new Order(command.OrderId);
			}

			order.UpdateOrderReportCreated(command.AccountId, command.PickupDate,
				command.PickupAddress, command.DropOffAddress, command.Settings, command.EstimatedFare,
				command.UserAgent, command.ClientLanguageCode, command.UserLatitude, command.UserLongitude,
				command.UserNote, command.ClientVersion, command.IsChargeAccountPaymentWithCardOnFile,
				command.CompanyKey, command.CompanyName, command.Market, command.IsPrepaid, command.BookingFees, command.Error, command.TipIncentive,
                command.IbsInformationNote, command.Fare, command.IbsAccountId, command.Prompts, command.PromptsLength,
                command.PromotionId, command.IsFutureBooking, command.ReferenceDataCompanyList, command.IbsOrderId,
                command.OriginatingIpAddress, command.KountSessionId);

			if (command.Payment.PayWithCreditCard)
			{
				var payment = Mapper.Map<PaymentInformation>(command.Payment);
				order.SetPaymentInformation(payment);
			}

			_repository.Save(order, command.Id.ToString());
		}

        public void Handle(PairForPayment command)
        {
            var order = _repository.Find(command.OrderId);
            order.Pair(command.Medallion, command.DriverId, command.PairingToken, command.PairingCode,
                command.TokenOfCardToBeUsedForPayment, command.AutoTipAmount, command.AutoTipPercentage);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(RateOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.RateOrder(command.AccountId, command.Note, command.RatingScores);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(RemoveOrderFromHistory command)
        {
            var order = _repository.Find(command.OrderId);
            order.RemoveFromHistory();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(UnpairForPayment command)
        {
            var order = _repository.Find(command.OrderId);
            order.Unpair();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(PrepareOrderForNextDispatch command)
        {
            var order = _repository.Find(command.OrderId);
            order.PrepareForNextDispatch(command.DispatchCompanyName, command.DispatchCompanyKey);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(InitiateIbsOrderSwitch command)
        {
            var order = _repository.Get(command.NewOrderCommand.OrderId);
            order.InitiateIbsOrderSwitch(command.NewIbsAccountId, command.NewOrderCommand);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(SwitchOrderToNextDispatchCompany command)
        {
            var order = _repository.Find(command.OrderId);
            order.SwitchOrderToNextDispatchCompany(command.IBSOrderId, command.CompanyKey, command.CompanyName, command.Market, command.HasChangedBackToPaymentInCar);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(IgnoreDispatchCompanySwitch command)
        {
            var order = _repository.Find(command.OrderId);
            order.IgnoreDispatchCompanySwitch();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(AddIbsOrderInfoToOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.AddIbsOrderInfo(command.IBSOrderId);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(CancelOrderBecauseOfError command)
        {
            var order = _repository.Find(command.OrderId);
            order.CancelBecauseOfError(command.ErrorCode, command.ErrorDescription);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(SaveTemporaryOrderCreationInfo command)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new TemporaryOrderCreationInfoDetail
                {
                    OrderId = command.OrderId,
                    SerializedOrderCreationInfo = command.SerializedOrderCreationInfo
                });
            }
        }

        public void Handle(SaveTemporaryOrderPaymentInfo command)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new TemporaryOrderPaymentInfoDetail
                {
                    OrderId = command.OrderId,
                    Cvv = command.Cvv
                });
            }
        }

        public void Handle(MarkPrepaidOrderAsSuccessful command)
        {
            var order = _repository.Find(command.OrderId);

            order.UpdatePrepaidOrderPaymentInfo(command.OrderId, command.TotalAmount, command.MeterAmount, command.TaxAmount,
                command.TipAmount, command.TransactionId, command.Provider, command.Type);

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(UpdateRefundedOrder command)
        {
            var order = _repository.Get(command.OrderId);
            order.RefundedOrderUpdated(command.IsSuccessful, command.Message);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(CreateOrderForManualRideLinqPair command)
        {
			var order = new Order(command.OrderId);

			order.UpdateOrderManuallyPairedForRideLinq(command.AccountId, command.PairingDate, command.PairingCode, command.PairingToken,
                command.PickupAddress, command.UserAgent, command.ClientLanguageCode, command.ClientVersion, command.Distance, command.Total,
                command.Fare, command.FareAtAlternateRate, command.Tax, command.Tip, command.Toll, command.Extra, 
                command.Surcharge, command.RateAtTripStart, command.RateAtTripEnd, command.RateChangeTime, command.Medallion, command.DeviceName,
				command.TripId, command.DriverId, command.AccessFee, command.LastFour, command.OriginatingIpAddress, command.KountSessionId);

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(UnpairOrderForManualRideLinq command)
        {
            var order = _repository.Get(command.OrderId);
            order.UnpairFromRideLinq();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(UpdateTripInfoInOrderForManualRideLinq command)
        {
            var order = _repository.Get(command.OrderId);
            order.UpdateRideLinqTripInfo(command.Distance,command.Total, command.Fare, command.FareAtAlternateRate, command.Tax,
                command.Tip, command.TollTotal, command.Extra,command.Surcharge,command.RateAtTripStart, command.RateAtTripEnd, 
                command.RateChangeTime, command.StartTime, command.EndTime, command.PairingToken, command.TripId, command.DriverId, command.AccessFee,
                command.LastFour, command.Tolls, command.LastLatitudeOfVehicle, command.LastLongitudeOfVehicle, command.PairingError);

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(UpdateAutoTip command)
        {
            var order = _repository.Get(command.OrderId);
            order.UpdateAutoTip(command.AutoTipPercentage);

            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(LogOriginalEta command)
        {
            var order = _repository.Get(command.OrderId);
            order.LogOriginalEta(command.OriginalEta);

            _repository.Save(order, command.Id.ToString());
        }
        public void Handle(UpdateOrderNotificationDetail command)
        {
            var order = _repository.Get(command.OrderId);
            order.UpdateOrderNotificationDetail(command);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(PayGratuity command)
        {
            var order = _repository.Find(command.OrderId);
            order.PayGratuity(command);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(UpdateOrderGratuity command)
        {
            var order = _repository.Find(command.OrderId);
            order.UpdateOrderGratuity(command);
            _repository.Save(order, command.Id.ToString());
        }
        
        public void Handle(UpdateOrderInTrip command)
        {
            var order = _repository.Get(command.OrderId);
            order.UpdateOrderInTrip(command);
            _repository.Save(order, command.Id.ToString());
        }
    }
}