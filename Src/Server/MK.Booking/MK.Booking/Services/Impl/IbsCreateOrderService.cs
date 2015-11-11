using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Services.Impl
{
    public class IbsCreateOrderService : IIbsCreateOrderService
    {
        private readonly IServerSettings _serverSettings;
        private readonly IVehicleTypeDao _vehicleTypeDao;
        private readonly IAccountDao _accountDao;
        private readonly ILogger _logger;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;
        private readonly IDispatcherService _dispatcherService;
        private readonly ICommandBus _commandBus;

        public IbsCreateOrderService(IServerSettings serverSettings,
            IVehicleTypeDao vehicleTypeDao,
            IAccountDao accountDao,
            ILogger logger,
            IIBSServiceProvider ibsServiceProvider,
            IUpdateOrderStatusJob updateOrderStatusJob,
            IDispatcherService dispatcherService,
            ICommandBus commandBus)
        {
            _serverSettings = serverSettings;
            _vehicleTypeDao = vehicleTypeDao;
            _accountDao = accountDao;
            _logger = logger;
            _ibsServiceProvider = ibsServiceProvider;
            _updateOrderStatusJob = updateOrderStatusJob;
            _dispatcherService = dispatcherService;
            _commandBus = commandBus;
        }

        public IBSOrderResult CreateIbsOrder(Guid accountId, Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString,
            string companyKey, int ibsAccountId, string name, string phone, int passengers, int? vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare, double? tipIncentive, bool isHailRequest = false, int? companyFleetId = null)
        {
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                // Fake IBS order id
                return new IBSOrderResult
                {
                    OrderKey = new OrderKey
                    {
                        IbsOrderId = new Random(Guid.NewGuid().GetHashCode()).Next(90000, 90000000),
                        TaxiHailOrderId = orderId
                    }
                };
            }

            var ibsOrderParams = PrepareForIbsOrder(chargeTypeId, pickupAddress, dropOffAddress, accountNumberString,
                customerNumberString, referenceDataCompanyList, market, requestProviderId);

            var dispatcherSettings = _dispatcherService.GetSettings(market, isHailRequest: isHailRequest);

            if (dispatcherSettings.NumberOfOffersPerCycle == 0)
            {
                // IBS is handling the dispatch
                var orderResult = _ibsServiceProvider.Booking(companyKey).CreateOrder(
                    orderId,
                    ibsOrderParams.ProviderId,
                    ibsAccountId,
                    name,
                    phone,
                    passengers,
                    vehicleTypeId,
                    ibsOrderParams.IbsChargeTypeId,
                    ibsInformationNote,
                    pickupDate,
                    ibsOrderParams.IbsPickupAddress,
                    ibsOrderParams.IbsDropOffAddress,
                    accountNumberString,
                    ibsOrderParams.CustomerNumber,
                    prompts,
                    promptsLength,
                    ibsOrderParams.DefaultVehicleTypeId,
                    tipIncentive,
                    dispatcherSettings.DurationOfOfferInSeconds,
                    fare);

                return Mapper.Map<IBSOrderResult>(orderResult);
            }

            return _dispatcherService.Dispatch(accountId, orderId, ibsOrderParams, new BestAvailableCompany { CompanyKey = companyKey, FleetId = companyFleetId},
                dispatcherSettings, accountNumberString, ibsAccountId, name, phone, passengers,
                vehicleTypeId, ibsInformationNote, pickupDate, prompts, promptsLength, market, fare, tipIncentive, isHailRequest);
        }

        public IbsOrderParams PrepareForIbsOrder(int? chargeTypeId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString,
            IList<ListItem> referenceDataCompanyList, string market, int? requestProviderId)
        {
            int? ibsChargeTypeId;

            if (chargeTypeId == ChargeTypes.CardOnFile.Id
                || chargeTypeId == ChargeTypes.PayPal.Id)
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypeCardOnFileId;
            }
            else if (chargeTypeId == ChargeTypes.Account.Id)
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypeChargeAccountId;
            }
            else
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypePaymentInCarId;
            }

            var ibsPickupAddress = Mapper.Map<IbsAddress>(pickupAddress);
            var ibsDropOffAddress = dropOffAddress != null && dropOffAddress.IsValid()
                ? Mapper.Map<IbsAddress>(dropOffAddress)
                : null;

            var customerNumber = GetCustomerNumber(accountNumberString, customerNumberString);

            var defaultVehicleType = _vehicleTypeDao.GetAll().FirstOrDefault();
            var defaultVehicleTypeId = defaultVehicleType != null ? defaultVehicleType.ReferenceDataVehicleId : -1;

            var defaultCompany = referenceDataCompanyList.FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                    ?? referenceDataCompanyList.FirstOrDefault();

            var providerId = market.HasValue() && referenceDataCompanyList.Any() && defaultCompany != null
                    ? defaultCompany.Id
                    : requestProviderId;

            return new IbsOrderParams
            {
                CustomerNumber = customerNumber,
                DefaultVehicleTypeId = defaultVehicleTypeId,
                IbsChargeTypeId = ibsChargeTypeId,
                IbsPickupAddress = ibsPickupAddress,
                IbsDropOffAddress = ibsDropOffAddress,
                ProviderId = providerId
            };
        }

        public void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, Guid accountId)
        {
            var ibsAccountId = _accountDao.GetIbsAccountId(accountId, companyKey);
            if (ibsAccountId.HasValue)
            {
                _dispatcherService.CancelIbsOrder(ibsOrderId, companyKey, phone, ibsAccountId.Value);
            }
        }

        public void UpdateOrderStatusAsync(Guid orderId)
        {
            _logger.LogMessage(string.Format("Starting status updater for order {0}", orderId));

            new TaskFactory().StartNew(() => _updateOrderStatusJob.CheckStatus(orderId));
        }

        private int? GetCustomerNumber(string accountNumber, string customerNumber)
        {
            if (!accountNumber.HasValue() || !customerNumber.HasValue())
            {
                return null;
            }

            int result;
            if (int.TryParse(customerNumber, out result))
            {
                return result;
            }

            return null;
        }
    }
}
