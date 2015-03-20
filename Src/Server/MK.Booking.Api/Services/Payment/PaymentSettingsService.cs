using System;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PaymentSettingsService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationDao _configurationDao;
        private readonly ILogger _logger;
        private readonly IPayPalServiceFactory _paylServiceFactory;

        public PaymentSettingsService(ICommandBus commandBus, IConfigurationDao configurationDao, ILogger logger, IPayPalServiceFactory paylServiceFactory)
        {
            _logger = logger;
            _paylServiceFactory = paylServiceFactory;
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