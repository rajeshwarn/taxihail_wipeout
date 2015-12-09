(function () {

    TaxiHail.PaymentSettings = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/settings/payments/server",

        save: function (settings) {

            var serializedData = JSON.stringify({
                serverPaymentSettings: settings
            });

            return $.ajax({
                type: 'POST',
                url: this.urlRoot,
                data: serializedData,
                contentType: 'application/json'
            });
        },


        testPayPalSandbox: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/payPal/sandbox",
                data: JSON.stringify({
                    serverCredentials: settings.payPalServerSettings.sandboxCredentials,
                    clientCredentials: settings.payPalClientSettings.sandboxCredentials
                }),
                contentType: 'application/json'
            });
        },
        
        testPayPalProduction: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/payPal/production",
                data: JSON.stringify({
                    serverCredentials: settings.payPalServerSettings.credentials,
                    clientCredentials: settings.payPalClientSettings.credentials
                }),
                contentType: 'application/json'
            });
        },

        testBraintree: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/braintree",
                data: JSON.stringify({
                    braintreeServerSettings: settings.braintreeServerSettings,
                    braintreeClientSettings: settings.braintreeClientSettings
                }),
                contentType: 'application/json'
            });
        },

        testCmt: function (settings, serviceType) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/cmt",
                data: JSON.stringify({
                    CmtPaymentSettings: settings.cmtPaymentSettings,
                    ServiceType: serviceType
                }),
                contentType: 'application/json'
            });
        },

        testMoneris: function(settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/moneris",
                data: JSON.stringify({
                    MonerisPaymentSettings: settings.monerisPaymentSettings
                }),
                contentType: 'application/json'
            });
        }

    });

}());