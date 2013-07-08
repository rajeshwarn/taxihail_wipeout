(function(){

    var Controller = TaxiHail.PaymentSettingsController = TaxiHail.Controller.extend({
        initialize: function() {
            this.settings = new TaxiHail.PaymentSettings();
            $.when(this.settings.fetch())
                    .then(this.ready);
        },

        index: function() {
            return new TaxiHail.ManagePaymentSettingsView({
                model: this.settings
            });
        }
    });
}());