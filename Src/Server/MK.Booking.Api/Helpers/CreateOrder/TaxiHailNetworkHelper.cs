using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using CustomerPortal.Client;
using Infrastructure.Messaging;
using log4net;

namespace apcurium.MK.Booking.Api.Helpers.CreateOrder
{
    public class TaxiHailNetworkHelper
    {
        private readonly IServerSettings _serverSettings;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly ICommandBus _commandBus;
        private readonly ILog _logger;

        public TaxiHailNetworkHelper(IServerSettings serverSettings, ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient, ICommandBus commandBus, ILog logger)
        {
            _serverSettings = serverSettings;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _commandBus = commandBus;
            _logger = logger;
        }

        internal bool FetchCompanyPaymentSettings(string companyKey)
        {
            try
            {
                var paymentSettings = _serverSettings.GetPaymentSettings();
                var companyPaymentSettings = _taxiHailNetworkServiceClient.GetPaymentSettings(companyKey);

                // Mobile will always keep local settings. The only values that needs to be overridden are the payment providers settings.
                paymentSettings.Id = Guid.NewGuid();
                paymentSettings.CompanyKey = companyKey;
                paymentSettings.PaymentMode = companyPaymentSettings.PaymentMode;
                paymentSettings.BraintreeServerSettings = new BraintreeServerSettings
                {
                    IsSandbox = companyPaymentSettings.BraintreePaymentSettings.IsSandbox,
                    MerchantId = companyPaymentSettings.BraintreePaymentSettings.MerchantId,
                    PrivateKey = companyPaymentSettings.BraintreePaymentSettings.PrivateKey,
                    PublicKey = companyPaymentSettings.BraintreePaymentSettings.PublicKey
                };
                paymentSettings.BraintreeClientSettings = new BraintreeClientSettings
                {
                    ClientKey = companyPaymentSettings.BraintreePaymentSettings.ClientKey
                };
                paymentSettings.MonerisPaymentSettings = new MonerisPaymentSettings
                {
                    IsSandbox = companyPaymentSettings.MonerisPaymentSettings.IsSandbox,
                    ApiToken = companyPaymentSettings.MonerisPaymentSettings.ApiToken,
                    BaseHost = companyPaymentSettings.MonerisPaymentSettings.BaseHost,
                    SandboxHost = companyPaymentSettings.MonerisPaymentSettings.SandboxHost,
                    StoreId = companyPaymentSettings.MonerisPaymentSettings.StoreId
                };
                paymentSettings.CmtPaymentSettings = new CmtPaymentSettings
                {
                    BaseUrl = companyPaymentSettings.CmtPaymentSettings.BaseUrl,
                    ConsumerKey = companyPaymentSettings.CmtPaymentSettings.ConsumerKey,
                    ConsumerSecretKey = companyPaymentSettings.CmtPaymentSettings.ConsumerSecretKey,
                    CurrencyCode = companyPaymentSettings.CmtPaymentSettings.CurrencyCode,
                    FleetToken = companyPaymentSettings.CmtPaymentSettings.FleetToken,
                    IsManualRidelinqCheckInEnabled = companyPaymentSettings.CmtPaymentSettings.IsManualRidelinqCheckInEnabled,
                    IsSandbox = companyPaymentSettings.CmtPaymentSettings.IsSandbox,
                    Market = companyPaymentSettings.CmtPaymentSettings.Market,
                    MobileBaseUrl = companyPaymentSettings.CmtPaymentSettings.MobileBaseUrl,
                    SandboxBaseUrl = companyPaymentSettings.CmtPaymentSettings.SandboxBaseUrl,
                    SandboxMobileBaseUrl = companyPaymentSettings.CmtPaymentSettings.SandboxMobileBaseUrl
                };

                // Save/update company settings
                _commandBus.Send(new UpdatePaymentSettings
                {
                    ServerPaymentSettings = paymentSettings
                });

                return companyPaymentSettings.PaymentMode == PaymentMethod.Cmt
                    || companyPaymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("An error occurred when trying to get PaymentSettings for company {0}", companyKey));
                _logger.Error(ex);

                return false;
            }
        }

        internal void UpdateVehicleTypeFromMarketData(BookingSettings bookingSettings, string marketCompanyId)
        {
            if (!bookingSettings.VehicleTypeId.HasValue)
            {
                // Nothing to do
                return;
            }

            try
            {
                // Get the vehicle type defined for the market of the company
                var matchingMarketVehicle = _taxiHailNetworkServiceClient.GetAssociatedMarketVehicleType(marketCompanyId, bookingSettings.VehicleTypeId.Value);
                if (matchingMarketVehicle != null)
                {
                    // Update the vehicle type info using the vehicle id from the IBS of that company
                    bookingSettings.VehicleType = matchingMarketVehicle.Name;
                    bookingSettings.VehicleTypeId = matchingMarketVehicle.ReferenceDataVehicleId;
                }
                else
                {
                    // No match found
                    bookingSettings.VehicleType = null;
                    bookingSettings.VehicleTypeId = null;

                    _logger.Info(string.Format("No match found for GetAssociatedMarketVehicleType for company {0}. Maybe no vehicles were linked via the admin panel?", marketCompanyId));
                }
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("An error occurred when trying to get GetAssociatedMarketVehicleType for company {0}", marketCompanyId));
                _logger.Error(ex);
            }
        }
    }
}
