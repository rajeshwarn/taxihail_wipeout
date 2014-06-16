(function () {

    TaxiHail.PaymentSettings = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/settings/payments/server",

        save: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot,
                data: JSON.stringify({
                    serverPaymentSettings: settings
                }),
                contentType: 'application/json'
            });
        },


        testPayPalSandbox: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/payPal/sandbox",
                data: JSON.stringify({
                    credentials: settings.payPalServerSettings.sandboxCredentials
                }),
                contentType: 'application/json'
            });
        },
        
        testPayPalProduction: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/payPal/production",
                data: JSON.stringify({
                    credentials: settings.payPalServerSettings.credentials
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

        testCmt: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test/cmt",
                data: JSON.stringify({
                    CmtPaymentSettings: settings.cmtPaymentSettings
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