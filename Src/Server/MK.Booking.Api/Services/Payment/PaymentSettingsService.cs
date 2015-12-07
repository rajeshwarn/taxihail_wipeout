using System;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Resources.Payment;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Cryptography;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PaymentSettingsService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationDao _configurationDao;
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;
        private readonly IPayPalServiceFactory _paylServiceFactory;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;

        public PaymentSettingsService(ICommandBus commandBus,
            IConfigurationDao configurationDao,
            ILogger logger,
            IServerSettings serverSettings,
            IPayPalServiceFactory paylServiceFactory,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient)
        {
            _logger = logger;
            _serverSettings = serverSettings;
            _paylServiceFactory = paylServiceFactory;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _commandBus = commandBus;
            _configurationDao = configurationDao;
        }

        public PaymentSettingsResponse Get(PaymentSettingsRequest request)
        {
            return new PaymentSettingsResponse
            {
                ClientPaymentSettings = _configurationDao.GetPaymentSettings()
            };
        }

		public Dictionary<string, string> Get(EncryptedPaymentSettingsRequest request)
		{
			var paymentSettings = _configurationDao.GetPaymentSettings();

		    var type = typeof (ClientPaymentSettings);

		    var settings =type 
                .GetAllProperties()
                // Only properties with a get and set should be sent
                .Where(property => property.Value.CanRead && property.Value.CanWrite)
                .Select(setting => setting.Key)
                .Select(propertyName => ExtractPropertyValue(paymentSettings, propertyName))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			SettingsEncryptor.SwitchEncryptionStringsDictionary(type, null, settings, true);

			return settings;
		}

        private static KeyValuePair<string, string> ExtractPropertyValue(ServerPaymentSettings paymentSettings, string propertyName)
        {
            var settingValue = paymentSettings.GetNestedPropertyValue(propertyName);
            var settingStringValue = settingValue.SelectOrDefault(value => value.ToString(), string.Empty);


            settingStringValue = settingStringValue.IsBool()
                ? settingStringValue.ToLowerInvariant()
                : settingStringValue;

            return new KeyValuePair<string, string>(propertyName, settingStringValue);
        }

        public ServerPaymentSettingsResponse Get(ServerPaymentSettingsRequest request)
        {
            var settings = _configurationDao.GetPaymentSettings();
            return new ServerPaymentSettingsResponse
            {
                ServerPaymentSettings = settings
            };
        }

        public void Post(UpdateServerPaymentSettingsRequest request)
        {
            if (request.ServerPaymentSettings.IsPayInTaxiEnabled &&
                request.ServerPaymentSettings.PaymentMode == PaymentMethod.None)
            {
                throw new ArgumentException("Please Select a payment setting");
            }

            _taxiHailNetworkServiceClient.UpdatePaymentSettings(_serverSettings.ServerData.TaxiHail.ApplicationKey,
                    new CompanyPaymentSettings
                    {
                        PaymentMode = request.ServerPaymentSettings.PaymentMode,
                        BraintreePaymentSettings = new BraintreePaymentSettings
                        {
                            ClientKey = request.ServerPaymentSettings.BraintreeClientSettings.ClientKey,
                            IsSandbox = request.ServerPaymentSettings.BraintreeServerSettings.IsSandbox,
                            MerchantId = request.ServerPaymentSettings.BraintreeServerSettings.MerchantId,
                            PrivateKey = request.ServerPaymentSettings.BraintreeServerSettings.PrivateKey,
                            PublicKey = request.ServerPaymentSettings.BraintreeServerSettings.PublicKey
                        },
                        MonerisPaymentSettings = new MonerisPaymentSettings
                        {
                            IsSandbox = request.ServerPaymentSettings.MonerisPaymentSettings.IsSandbox,
                            ApiToken = request.ServerPaymentSettings.MonerisPaymentSettings.ApiToken,
                            BaseHost = request.ServerPaymentSettings.MonerisPaymentSettings.BaseHost,
                            SandboxHost = request.ServerPaymentSettings.MonerisPaymentSettings.SandboxHost,
                            StoreId = request.ServerPaymentSettings.MonerisPaymentSettings.StoreId
                        },
                        CmtPaymentSettings = new CmtPaymentSettings
                        {
                            BaseUrl = request.ServerPaymentSettings.CmtPaymentSettings.BaseUrl,
                            ConsumerKey = request.ServerPaymentSettings.CmtPaymentSettings.ConsumerKey,
                            ConsumerSecretKey = request.ServerPaymentSettings.CmtPaymentSettings.ConsumerSecretKey,
                            CurrencyCode = request.ServerPaymentSettings.CmtPaymentSettings.CurrencyCode,
                            FleetToken = request.ServerPaymentSettings.CmtPaymentSettings.FleetToken,
                            IsManualRidelinqCheckInEnabled = request.ServerPaymentSettings.CmtPaymentSettings.IsManualRidelinqCheckInEnabled,
                            IsSandbox = request.ServerPaymentSettings.CmtPaymentSettings.IsSandbox,
                            Market = request.ServerPaymentSettings.CmtPaymentSettings.Market,
                            MobileBaseUrl = request.ServerPaymentSettings.CmtPaymentSettings.MobileBaseUrl,
                            SandboxBaseUrl = request.ServerPaymentSettings.CmtPaymentSettings.SandboxBaseUrl,
                            SandboxMobileBaseUrl = request.ServerPaymentSettings.CmtPaymentSettings.SandboxMobileBaseUrl
                        }
                    })
                    .HandleErrors();

            _commandBus.Send(new UpdatePaymentSettings {ServerPaymentSettings = request.ServerPaymentSettings});
        }

        public TestServerPaymentSettingsResponse Post(TestPayPalProductionSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Paypal Production Credentials are invalid"
            };

            try
            {
                if (_paylServiceFactory.GetInstance().TestCredentials(request.ClientCredentials, request.ServerCredentials, false))
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "Paypal Production Credentials are valid"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += "\n" + e.Message;
            }
            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestPayPalSandboxSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Paypal Sandbox Credentials are invalid"
            };

            try
            {
                if (_paylServiceFactory.GetInstance().TestCredentials(request.ClientCredentials, request.ServerCredentials, true))
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "Paypal Sandbox Credentials are valid"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += "\n" + e.Message;
            }
            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestBraintreeSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Braintree Settings are invalid"
            };

            try
            {
                if (BraintreeClientPaymentService.TestClient(request.BraintreeServerSettings, request.BraintreeClientSettings))
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "Braintree Settings are valid"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += "\n" + e.Message;
            }

            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestCmtSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "CMT Settings are invalid"
            };

            try
            {
                var cc = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Cmt).Visa;
                var result = CmtPaymentClient.TestClient(request.CmtPaymentSettings, cc.Number, cc.ExpirationDate, _logger);
                if (result)
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "CMT Settings are valid"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += "\n" + e.Message + "\n" + e;
            }

            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestMonerisSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Moneris Settings are invalid"
            };

            try
            {
                var cc = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Moneris).Visa;
                var result = MonerisServiceClient.TestClient(request.MonerisPaymentSettings, cc.Number, cc.ExpirationDate, _logger);
                if (result)
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "Moneris Settings are valid"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += "\n" + e.Message;
            }

            return response;
        }
    }
}