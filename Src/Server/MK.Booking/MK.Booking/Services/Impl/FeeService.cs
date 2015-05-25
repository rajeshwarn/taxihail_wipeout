using System;
using System.Threading;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using log4net;
using RestSharp.Extensions;

namespace apcurium.MK.Booking.Services.Impl
{
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(FeeService));

        public FeeService(IPaymentService paymentService, IAccountDao accountDao, IFeesDao feesDao, 
            IOrderDao orderDao, IOrderPaymentDao paymentDao, ICommandBus commandBus, IServerSettings serverSettings, 
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

        public void ChargeNoShowFeeIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            if (ibsOrderInfo.Status != VehicleStatuses.Common.NoShow)
            {
                return;
            }
            
            if (orderStatusDetail.IsPrepaid)
            {
                // Order is prepaid, if the user prepaid and decided not to show up, the fee is his fare already charged
                return;
            }
            
            var feesForMarket = _feesDao.GetMarketFees(orderStatusDetail.Market);
            var noShowFee = feesForMarket != null ? feesForMarket.NoShow : 0;

            if (noShowFee <= 0)
            {
                return;
            }

            Log.DebugFormat("No show fee of {0} will be charged for order {1}{2}.", 
                noShowFee, 
                ibsOrderInfo.IBSOrderId, 
                string.Format(orderStatusDetail.Market.HasValue() 
                        ? "in market {0}" 
                        : string.Empty, 
                    orderStatusDetail.Market));

            var account = _accountDao.FindById(orderStatusDetail.AccountId);
            if (!account.HasValidPaymentInformation)
            {
                Log.DebugFormat("No show fee cannot be charged for order {0} because the user has no payment method configured.", ibsOrderInfo.IBSOrderId);
                return;
            }

            try
            {
                // PreAuthorization
                var preAuthResponse = PreauthorizePaymentIfNecessary(orderStatusDetail.OrderId, noShowFee);
                if (preAuthResponse.IsSuccessful)
                {
                    // Commit
                    var paymentResult = CommitPayment(noShowFee, orderStatusDetail.OrderId, true, false);
                    if (paymentResult.IsSuccessful)
                    {
                        Log.DebugFormat("No show fee of amount {0} was charged for order {1}.", noShowFee, ibsOrderInfo.IBSOrderId);
                    }
                    else
                    {
                        throw new Exception(paymentResult.Message);
                    }
                }
                else
                {
                    throw new Exception(preAuthResponse.Message);
                }
            }
            catch (Exception ex)
            {
                Log.DebugFormat("Could not process no show fee for order {0}: {1}.", ibsOrderInfo.IBSOrderId, ex.Message);
                throw ex;
            }
        }

        private PreAuthorizePaymentResponse PreauthorizePaymentIfNecessary(Guid orderId, decimal amount, string cvv = null)
        {
            // Check payment instead of PreAuth setting, because we do not preauth in the cases of future bookings
            var paymentInfo = _paymentDao.FindByOrderId(orderId);
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

            var result = _paymentService.PreAuthorize(orderId, account, amount, cvv: cvv);
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
                    OverdueAmount = amount,
                    TransactionId = result.TransactionId,
                    TransactionDate = result.TransactionDate
                });
            }

            return result;
        }

        private CommitPreauthorizedPaymentResponse CommitPayment(decimal feeAmount, Guid orderId, bool isNoShowFee, bool isCancellationFee)
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

            var paymentDetail = _paymentDao.FindByOrderId(orderId);
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
                    if (feeAmount > 0)
                    {
                        paymentProviderServiceResponse = _paymentService.CommitPayment(orderId, account, paymentDetail.PreAuthorizedAmount, feeAmount, feeAmount, 0, paymentDetail.TransactionId);
                        message = paymentProviderServiceResponse.Message;
                    }
                    else
                    {
                        // void preauth if it exists
                        _paymentService.VoidPreAuthorization(orderId);

                        paymentProviderServiceResponse.IsSuccessful = true;
                    }
                }

                if (paymentProviderServiceResponse.IsSuccessful)
                {
                    // Payment completed

                    var fareObject = FareHelper.GetFareFromAmountInclTax(Convert.ToDouble(feeAmount), _serverSettings.ServerData.VATIsEnabled ? _serverSettings.ServerData.VATPercentage : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        AccountId = account.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(orderDetail.Id),
                        Amount = feeAmount,
                        MeterAmount = Convert.ToDecimal(fareObject.AmountExclTax),
                        TipAmount = Convert.ToDecimal(0),
                        TaxAmount = Convert.ToDecimal(fareObject.TaxAmount),
                        IsNoShowFee = isNoShowFee,
                        IsCancellationFee = isCancellationFee,
                        AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                        TransactionId = paymentProviderServiceResponse.TransactionId
                    });
                }
                else
                {
                    // Void PreAuth because commit failed
                    _paymentService.VoidPreAuthorization(orderId);

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
                            OverdueAmount = feeAmount,
                            TransactionId = paymentProviderServiceResponse.TransactionId,
                            TransactionDate = paymentProviderServiceResponse.TransactionDate
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