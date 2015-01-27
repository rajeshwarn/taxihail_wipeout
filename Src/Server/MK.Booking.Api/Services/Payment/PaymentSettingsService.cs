using System;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PaymentSettingsService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationDao _configurationDao;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly IPayPalServiceFactory _paylServiceFactory;

        public PaymentSettingsService(ICommandBus commandBus, IConfigurationDao configurationDao,
            IServerSettings serverSettings, ILogger logger, IPayPalServiceFactory paylServiceFactory)
        {
            _logger = logger;
            _paylServiceFactory = paylServiceFactory;
            _commandBus = commandBus;
            _configurationDao = configurationDao;
            _serverSettings = serverSettings;
        }

        public PaymentSettingsResponse Get(PaymentSettingsRequest request)
        {
            return new PaymentSettingsResponse
            {
                ClientPaymentSettings = _configurationDao.GetPaymentSettings()
            };
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

            _commandBus.Send(new UpdatePaymentSettings {ServerPaymentSettings = request.ServerPaymentSettings});
        }

        public TestServerPaymentSettingsResponse Post(TestPayPalProductionSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Paypal Production Credentials are invalid\n"
            };

            try
            {
                if (_paylServiceFactory.GetInstance().TestClient(_serverSettings, RequestContext, request.ServerCredentials, false))
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "Paypal Production Credentials are valid\n"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += e.Message + "\n";
            }
            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestPayPalSandboxSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Paypal Sandbox Credentials are invalid\n"
            };

            try
            {
                if (_paylServiceFactory.GetInstance().TestClient(_serverSettings, RequestContext, request.ServerCredentials, true))
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "Paypal Sandbox Credentials are valid\n"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += e.Message + "\n";
            }
            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestBraintreeSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Braintree Settings are invalid\n"
            };

            try
            {
                if (BraintreeClientPaymentService.TestClient(request.BraintreeServerSettings, request.BraintreeClientSettings))
                {
                    return new TestServerPaymentSettingsResponse
                    {
                        IsSuccessful = true,
                        Message = "Braintree Settings are valid\n"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += e.Message + "\n";
            }

            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestCmtSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "CMT Settings are invalid\n"
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
                        Message = "CMT Settings are valid\n"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += e.Message + "\n" + e;
            }

            return response;
        }

        public TestServerPaymentSettingsResponse Post(TestMonerisSettingsRequest request)
        {
            var response = new TestServerPaymentSettingsResponse
            {
                IsSuccessful = false,
                Message = "Moneris Settings are invalid\n"
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
                        Message = "Moneris Settings are valid\n"
                    };
                }
            }
            catch (Exception e)
            {
                response.Message += e.Message + "\n";
            }

            return response;
        }
    }
}