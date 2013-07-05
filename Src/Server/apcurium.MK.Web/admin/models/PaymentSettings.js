(function () {

    TaxiHail.PaymentSettings = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/settings/payments/server",

        batchSave: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot,
                data: JSON.stringify({
                    serverPaymentSettings: settings
                }),
                contentType: 'application/json'
            });
        }
    });

}());