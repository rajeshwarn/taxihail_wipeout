using System;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using CMTServices;
using CMTServices.Responses;
using CustomerPortal.Client;
using CustomerPortal.Contract.Response;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Api.Helpers.CreateOrder
{
    public class TaxiHailNetworkHelper
    {
        private readonly IServerSettings _serverSettings;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;

        public TaxiHailNetworkHelper(IServerSettings serverSettings, ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient, ICommandBus commandBus, ILogger logger)
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
                _logger.LogMessage(string.Format("An error occurred when trying to get PaymentSettings for company {0}", companyKey));
                _logger.LogError(ex);

                return false;
            }
        }

        internal void UpdateVehicleTypeFromMarketData(BookingSettings bookingSettings, string marketCompanyId)
        {
            if (!bookingSettings.VehicleTypeId.HasValue || !marketCompanyId.HasValue())
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

                    _logger.LogMessage(string.Format("No match found for GetAssociatedMarketVehicleType for company {0}. Maybe no vehicles were linked via the admin panel?", marketCompanyId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(string.Format("An error occurred when trying to get GetAssociatedMarketVehicleType for company {0}", marketCompanyId));
                _logger.LogError(ex);
            }
        }

        internal BestAvailableCompany FindSpecificCompany(string market, CreateReportOrder createReportOrder, string orderCompanyKey = null, int? orderFleetId = null, double? latitude = null, double? longitude = null)
        {
            if (!orderCompanyKey.HasValue() && !orderFleetId.HasValue)
            {
                Exception createOrderException = new ArgumentNullException("You must at least provide a value for orderCompanyKey or orderFleetId");
                createReportOrder.Error = createOrderException.ToString();
                _commandBus.Send(createReportOrder);
                throw createOrderException;
            }

            var companyKey = _serverSettings.ServerData.TaxiHail.ApplicationKey;

            var fleets = market.HasValue()
                ? _taxiHailNetworkServiceClient.GetMarketFleets(companyKey, market).ToArray()
                : _taxiHailNetworkServiceClient.GetNetworkFleet(companyKey, latitude, longitude).ToArray();

            if (orderCompanyKey.HasValue())
            {
                var match = fleets.FirstOrDefault(f => f.CompanyKey == orderCompanyKey);
                if (match != null)
                {
                    return new BestAvailableCompany
                    {
                        CompanyKey = match.CompanyKey,
                        CompanyName = match.CompanyName,
                        FleetId = match.FleetId
                    };
                }
            }

            if (orderFleetId.HasValue)
            {
                var match = fleets.FirstOrDefault(f => f.FleetId == orderFleetId.Value);
                if (match != null)
                {
                    return new BestAvailableCompany
                    {
                        CompanyKey = match.CompanyKey,
                        CompanyName = match.CompanyName,
                        FleetId = match.FleetId
                    };
                }
            }

            // Nothing found
            return new BestAvailableCompany();
        }

        internal BestAvailableCompany FindBestAvailableCompany(string market, double? latitude, double? longitude)
        {
            if (!market.HasValue() || !latitude.HasValue || !longitude.HasValue)
            {
                // Do nothing if in home market or if we don't have position
                _logger.LogMessage("FindBestAvailableCompany - We are in local market (or lat/lng is null), skip honeybadger/geo and call local ibs first");
                return new BestAvailableCompany();
            }

            int? bestFleetId = null;
            const int searchExpendLimit = 10;
            var searchRadius = 2000; // In meters

            for (var i = 1; i < searchExpendLimit; i++)
            {
                var marketVehicles = GetAvailableVehiclesServiceClient(market)
                    .GetAvailableVehicles(market, latitude.Value, longitude.Value, searchRadius, null, true)
                    .ToArray();

                if (marketVehicles.Any())
                {
                    // Group vehicles by fleet
                    var vehiclesGroupedByFleet = marketVehicles.GroupBy(v => v.FleetId).Select(g => g.ToArray()).ToArray();

                    var result = vehiclesGroupedByFleet.FirstOrDefault() ?? new VehicleResponse[0];

                    // Take fleet with most number of available vehicles
                    foreach (var response in vehiclesGroupedByFleet.Skip(1))
                    {
                        if (response.Length > result.Length)
                        {
                            // this fleet has more vehicles
                            result = response;
                            continue;
                        }

                        if (response.Length == result.Length)
                        {
                            // two fleets have the same number of vehicles, take the one with the shortest ETA
                            if (result.Where(x => x.Eta.HasValue).Min(x => x.Eta) > response.Where(x => x.Eta.HasValue).Min(x => x.Eta))
                            {
                                result = response;
                            }
                        }
                    }
                    bestFleetId = result.First().FleetId;
                    break;
                }

                // Nothing found, extend search radius (total radius after 10 iterations: 3375m)
                searchRadius += (i * 25);
            }
            
            // Nothing found
            if (!bestFleetId.HasValue)
            {
                return new BestAvailableCompany();
            }

            var companyKey = _serverSettings.ServerData.TaxiHail.ApplicationKey;
            var marketFleets = _taxiHailNetworkServiceClient.GetMarketFleets(companyKey, market)
                .SelectOrDefault(markets => markets.ToArray(), new NetworkFleetResponse[0]);

            // Fallback: If for some reason, we cannot find a match for the best fleet id in the fleets
            // that were setup for the market, we take the first one
            var bestFleet = marketFleets.FirstOrDefault(f => f.FleetId == bestFleetId.Value)
                            ?? marketFleets.FirstOrDefault();
            
            // Nothing found
            if (bestFleet == null)
            {
                return new BestAvailableCompany();
            }

            return new BestAvailableCompany
            {
                CompanyKey = bestFleet.CompanyKey,
                CompanyName = bestFleet.CompanyName,
                FleetId = bestFleet.FleetId
            };
        }

        private BaseAvailableVehicleServiceClient GetAvailableVehiclesServiceClient(string market)
        {
            if (IsCmtGeoServiceMode(market))
            {
                return new CmtGeoServiceClient(_serverSettings, _logger);
            }

            return new HoneyBadgerServiceClient(_serverSettings, _logger);
        }

        private bool IsCmtGeoServiceMode(string market)
        {
            var externalMarketMode = market.HasValue() && _serverSettings.ServerData.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo;

            var internalMarketMode = !market.HasValue() && _serverSettings.ServerData.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo;

            return internalMarketMode || externalMarketMode;
        }
    }
}
