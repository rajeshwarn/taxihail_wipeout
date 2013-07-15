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


        test: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot + "/test",
                data: JSON.stringify({
                    serverPaymentSettings: settings
                }),
                contentType: 'application/json'
            });
        },
    });

}());