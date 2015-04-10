﻿#region

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
        ICommandHandler<SwitchOrderToNextDispatchCompany>,
        ICommandHandler<IgnoreDispatchCompanySwitch>,
        ICommandHandler<AddIbsOrderInfoToOrder>,
        ICommandHandler<CancelOrderBecauseOfError>,
        ICommandHandler<SaveTemporaryOrderCreationInfo>,
        ICommandHandler<MarkPrepaidOrderAsSuccessful>,
        ICommandHandler<UpdateRefundedOrder>,
        ICommandHandler<CreateOrderForManualRideLinqPair>,
        ICommandHandler<UnpairOrderForManualRideLinq>,
        ICommandHandler<UpdateTripInfoInOrderForManualRideLinq>
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
            order.ChangeStatus(command.Status, command.Fare, command.Tip, command.Toll, command.Tax);

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
            var order = new Order(command.OrderId, command.AccountId, command.PickupDate,
                command.PickupAddress, command.DropOffAddress, command.Settings, command.EstimatedFare,
                command.UserAgent, command.ClientLanguageCode, command.UserLatitude, command.UserLongitude,
                command.UserNote, command.ClientVersion, command.IsChargeAccountPaymentWithCardOnFile,
                command.CompanyKey, command.CompanyName, command.Market, command.IsPrepaid);

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
            order.RateOrder(command.Note, command.RatingScores);
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

        public void Handle(SwitchOrderToNextDispatchCompany command)
        {
            var order = _repository.Find(command.OrderId);
            order.SwitchOrderToNextDispatchCompany(command.IBSOrderId, command.CompanyKey, command.CompanyName, command.Market);
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
            order.CancelBecauseOfError(command.ErrorCode, command.ErrorDescription, command.WasPrepaid);
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

        public void Handle(MarkPrepaidOrderAsSuccessful command)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveWhere<TemporaryOrderCreationInfoDetail>(x => x.OrderId == command.OrderId);
                context.SaveChanges();
            }

            var order = _repository.Find(command.OrderId);

            order.UpdatePrepaidOrderPaymentInfo(command.OrderId, command.Amount, command.Meter, command.Tax,
                command.Tip, command.TransactionId, command.Provider, command.Type);

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
            var order = new Order(command.OrderId, command.AccountId, command.PairingDate, command.PairingCode, command.PairingToken,
                command.PickupAddress, command.UserAgent, command.ClientLanguageCode, command.ClientVersion, command.Distance, command.Total,
                command.Fare, command.FareAtAlternateRate, command.Tax, command.Tip, command.Toll, command.Extra, 
                command.Surcharge, command.RateAtTripStart, command.RateAtTripEnd, command.RateChangeTime, command.Medallion, command.TripId, command.DriverId);

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
                command.Tip, command.Toll, command.Extra,command.Surcharge,command.RateAtTripStart, command.RateAtTripEnd, 
                command.RateChangeTime ,command.EndTime, command.PairingToken, command.Medallion, command.TripId, command.DriverId);

            _repository.Save(order, command.Id.ToString());
        }
    }
}