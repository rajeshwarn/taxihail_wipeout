using System;
using System.Threading;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using RestSharp.Extensions;

namespace apcurium.MK.Booking.Services.Impl
{
    /// <summary>
    /// Fees only work with CMT for now
    /// </summary>
    public class FeeService : IFeeService
    {
        private readonly IPaymentService _paymentService;
        private readonly IAccountDao _accountDao;
        private readonly IFeesDao _feesDao;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;

        public FeeService(IPaymentService paymentService,
            IAccountDao accountDao,
            IFeesDao feesDao, 
            IOrderDao orderDao,
            IOrderPaymentDao paymentDao,
            ICommandBus commandBus,
            IServerSettings serverSettings, 
            ILogger logger)
        {
            _paymentService = paymentService;
            _accountDao = accountDao;
            _feesDao = feesDao;
            _orderDao = orderDao;
            _paymentDao = paymentDao;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
            _logger = logger;
        }

        public decimal? ChargeBookingFeesIfNecessary(OrderStatusDetail orderStatusDetail)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            if (orderStatusDetail.IsPrepaid
                || orderStatusDetail.CompanyKey == null // If booking is made on home company, booking fees will be included in same trip receipt
                || (paymentSettings.PaymentMode != PaymentMethod.Cmt &&
                    paymentSettings.PaymentMode != PaymentMethod.RideLinqCmt))
            {
                return null;
            }

            var feesForMarket = _feesDao.GetMarketFees(orderStatusDetail.Market);
            var bookingFees = feesForMarket != null
                ? feesForMarket.Booking
                : 0;

            if (bookingFees <= 0)
            {
                return null;
            }

            _logger.LogMessage("Booking fee of {0} will be charged for order {1}{2}.",
                bookingFees,
                orderStatusDetail.IBSOrderId,
                string.Format(orderStatusDetail.Market.HasValue()
                        ? "in market {0}"
                        : string.Empty,
                    orderStatusDetail.Market));

            var account = _accountDao.FindById(orderStatusDetail.AccountId);
            if (!account.HasValidPaymentInformation)
            {
                _logger.LogMessage("Booking fee cannot be charged for order {0} because the user has no payment method configured.", orderStatusDetail.IBSOrderId);
                return null;
            }

            try
            {
                // PreAuthorization
                var preAuthResponse = PreauthorizePaymentIfNecessary(orderStatusDetail.OrderId, bookingFees, FeeTypes.Booking);
                if (preAuthResponse.IsSuccessful)
                {
                    // Commit
                    var paymentResult = CommitPayment(bookingFees, bookingFees, orderStatusDetail.OrderId, FeeTypes.Booking);
                    if (paymentResult.IsSuccessful)
                    {
                        _logger.LogMessage("No show fee of amount {0} was charged for order {1}.", bookingFees, orderStatusDetail.IBSOrderId);
                        return bookingFees;
                    }

                    throw new Exception(paymentResult.Message);
                }

                throw new Exception(preAuthResponse.Message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Could not process no show fee for order {0}: {1}.", orderStatusDetail.IBSOrderId, ex.Message);
                return null;
            }
        }

        public decimal? ChargeNoShowFeeIfNecessary(OrderStatusDetail orderStatusDetail)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();
            
            if (orderStatusDetail.IsPrepaid
                || (paymentSettings.PaymentMode != PaymentMethod.Cmt &&
                    paymentSettings.PaymentMode != PaymentMethod.RideLinqCmt))
            {
                // If order is prepaid, if the user prepaid and decided not to show up, the fee is his fare already charged
                return null;
            }

            // As requested by MK, we need to charge booking fees on top of cancellation fees
            var bookingFees = _orderDao.FindById(orderStatusDetail.OrderId).BookingFees;
            var feesForMarket = _feesDao.GetMarketFees(orderStatusDetail.Market);
            var noShowFee = feesForMarket != null
                ? feesForMarket.NoShow + bookingFees
                : 0;

            if (noShowFee <= 0)
            {
                return null;
            }

            _logger.LogMessage("No show fee of {0} will be charged for order {1}{2}.", 
                noShowFee,
                orderStatusDetail.IBSOrderId, 
                string.Format(orderStatusDetail.Market.HasValue() 
                        ? "in market {0}" 
                        : string.Empty, 
                    orderStatusDetail.Market));

            var account = _accountDao.FindById(orderStatusDetail.AccountId);
            if (!account.HasValidPaymentInformation)
            {
                _logger.LogMessage("No show fee cannot be charged for order {0} because the user has no payment method configured.", orderStatusDetail.IBSOrderId);
                return null;
            }

            try
            {
                // PreAuthorization
                var preAuthResponse = PreauthorizePaymentIfNecessary(orderStatusDetail.OrderId, noShowFee, FeeTypes.NoShow);
                if (preAuthResponse.IsSuccessful)
                {
                    // Commit
                    var paymentResult = CommitPayment(noShowFee, bookingFees, orderStatusDetail.OrderId, FeeTypes.NoShow);
                    if (paymentResult.IsSuccessful)
                    {
                        _logger.LogMessage("No show fee of amount {0} was charged for order {1}.", noShowFee, orderStatusDetail.IBSOrderId);
                        return noShowFee;
                    }

                    throw new Exception(paymentResult.Message);
                }

                throw new Exception(preAuthResponse.Message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Could not process no show fee for order {0}: {1}.", orderStatusDetail.IBSOrderId, ex.Message);
                return null;
            }
        }

        public decimal? ChargeCancellationFeeIfNecessary(OrderStatusDetail orderStatusDetail)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            if (orderStatusDetail.IsPrepaid
                || (paymentSettings.PaymentMode != PaymentMethod.Cmt &&
                    paymentSettings.PaymentMode != PaymentMethod.RideLinqCmt))
            {
                return null;
            }

            var pairingDetail = _orderDao.FindOrderPairingById(orderStatusDetail.OrderId);

            var isPastNoFeeCancellationWindow =  pairingDetail != null // Pairing details might be null if the cancellation was done as a result of a network timeout.
				&& !pairingDetail.WasUnpaired // No cancellation fee for unpaired rides because the rider is either: already in the car (pay cash) or had his car hijacked
                && orderStatusDetail.TaxiAssignedDate.HasValue
                && orderStatusDetail.TaxiAssignedDate.Value.AddSeconds(_serverSettings.ServerData.CancellationFeesWindow) < DateTime.UtcNow;

            var bookingFees = _orderDao.FindById(orderStatusDetail.OrderId).BookingFees;
            var feesForMarket = _feesDao.GetMarketFees(orderStatusDetail.Market);
            var cancellationFee = feesForMarket != null
                ? feesForMarket.Cancellation + bookingFees
                : 0;

            if (cancellationFee <= 0 || !isPastNoFeeCancellationWindow)
            {
                return null;
            }

            _logger.LogMessage("Cancellation fee of {0} will be charged for order {1}{2}.",
                cancellationFee,
                orderStatusDetail.IBSOrderId,
                string.Format(orderStatusDetail.Market.HasValue()
                        ? "in market {0}"
                        : string.Empty,
                    orderStatusDetail.Market));

            var account = _accountDao.FindById(orderStatusDetail.AccountId);
            if (!account.HasValidPaymentInformation)
            {
                _logger.LogMessage("Cancellation fee cannot be charged for order {0} because the user has no payment method configured.", orderStatusDetail.IBSOrderId);
                return null;
            }

            try
            {
                // PreAuthorization
                var preAuthResponse = PreauthorizePaymentIfNecessary(orderStatusDetail.OrderId, cancellationFee, FeeTypes.Cancellation);
                if (preAuthResponse.IsSuccessful)
                {
                    // Commit
                    var paymentResult = CommitPayment(cancellationFee, bookingFees, orderStatusDetail.OrderId, FeeTypes.Cancellation);
                    if (paymentResult.IsSuccessful)
                    {
                        _logger.LogMessage("Cancellation fee of amount {0} was charged for order {1}.", cancellationFee, orderStatusDetail.IBSOrderId);
                        return cancellationFee;
                    }
                    throw new Exception(paymentResult.Message);
                }
                throw new Exception(preAuthResponse.Message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Could not process cancellation fee for order {0}: {1}.", orderStatusDetail.IBSOrderId, ex.Message);
                return null;
            }
        }

        private PreAuthorizePaymentResponse PreauthorizePaymentIfNecessary(Guid orderId, decimal totalFeeAmount, FeeTypes feeType, string companyKey = null)
        {
            // Check payment instead of PreAuth setting, because we do not preauth in the cases of future bookings
            var paymentInfo = _paymentDao.FindByOrderId(orderId, companyKey);
            if (paymentInfo != null)
            {
                // Already preauthorized on create order, do nothing
                return new PreAuthorizePaymentResponse { IsSuccessful = true };
            }

            var orderDetail = _orderDao.FindById(orderId);
            if (orderDetail == null)
            {
                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false,
                    Message = "Order not found"
                };
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            // Fees are collected by the local company
            var result = _paymentService.PreAuthorize(null, orderId, account, totalFeeAmount);
            if (result.IsSuccessful)
            {
                // Wait for OrderPaymentDetail to be created
                Thread.Sleep(500);
            }
            else if (result.IsDeclined)
            {
                // Deactivate credit card if it was declined
                _commandBus.Send(new ReactToPaymentFailure
                {
                    AccountId = orderDetail.AccountId,
                    OrderId = orderId,
                    IBSOrderId = orderDetail.IBSOrderId,
                    OverdueAmount = totalFeeAmount,
                    TransactionId = result.TransactionId,
                    TransactionDate = result.TransactionDate,
                    FeeType = feeType
                });
            }

            return result;
        }

        private CommitPreauthorizedPaymentResponse CommitPayment(decimal totalFeeAmount, decimal bookingFees, Guid orderId, FeeTypes feeType, string companyKey = null)
        {
            var orderDetail = _orderDao.FindById(orderId);
            if (orderDetail == null)
            {
                throw new Exception("Order not found");
            }

            if (orderDetail.IBSOrderId == null)
            {
                throw new Exception("Order has no IBSOrderId");
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            var paymentDetail = _paymentDao.FindByOrderId(orderId, companyKey);
            if (paymentDetail == null)
            {
                throw new Exception("Payment not found");
            }

            var paymentProviderServiceResponse = new CommitPreauthorizedPaymentResponse
            {
                TransactionId = paymentDetail.TransactionId
            };

            try
            {
                var message = string.Empty;

                if (paymentDetail.IsCompleted)
                {
                    message = "Order already paid or payment currently processing";
                }
                else
                {
                    if (totalFeeAmount > 0)
                    {
                        // Fees are collected by the local company
                        paymentProviderServiceResponse = _paymentService.CommitPayment(null, orderId, account, paymentDetail.PreAuthorizedAmount, totalFeeAmount, totalFeeAmount, 0, paymentDetail.TransactionId);
                        message = paymentProviderServiceResponse.Message;
                    }
                    else
                    {
                        // void preauth if it exists
                        _paymentService.VoidPreAuthorization(null, orderId);

                        paymentProviderServiceResponse.IsSuccessful = true;
                    }
                }

                if (paymentProviderServiceResponse.IsSuccessful)
                {
                    // Payment completed

                    var fareObject = FareHelper.GetFareFromAmountInclTax(Convert.ToDouble(totalFeeAmount), _serverSettings.ServerData.VATIsEnabled ? _serverSettings.ServerData.VATPercentage : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        AccountId = account.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(companyKey, orderDetail.Id),
                        TotalAmount = totalFeeAmount,
                        MeterAmount = Convert.ToDecimal(fareObject.AmountExclTax),
                        TipAmount = Convert.ToDecimal(0),
                        TaxAmount = Convert.ToDecimal(fareObject.TaxAmount),
                        AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                        TransactionId = paymentProviderServiceResponse.TransactionId,
                        FeeType = feeType,
                        BookingFees = bookingFees
                    });
                }
                else
                {
                    // Void PreAuth because commit failed
                    _paymentService.VoidPreAuthorization(null, orderId);

                    // Payment error
                    _commandBus.Send(new LogCreditCardError
                    {
                        PaymentId = paymentDetail.PaymentId,
                        Reason = message
                    });

                    if (paymentProviderServiceResponse.IsDeclined)
                    {
                        _commandBus.Send(new ReactToPaymentFailure
                        {
                            AccountId = account.Id,
                            OrderId = orderId,
                            IBSOrderId = orderDetail.IBSOrderId,
                            OverdueAmount = totalFeeAmount,
                            TransactionId = paymentProviderServiceResponse.TransactionId,
                            TransactionDate = paymentProviderServiceResponse.TransactionDate,
                            FeeType = feeType
                        });
                    }
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    IsSuccessful = paymentProviderServiceResponse.IsSuccessful,
                    Message = paymentProviderServiceResponse.IsSuccessful ? "Success" : message
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("Error during fee payment " + e);
                _logger.LogError(e);

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    Message = e.Message
                };
            }
        }
    }
}