(function(){

    var Controller = TaxiHail.CompanySettingsController = TaxiHail.Controller.extend({
        initialize: function() {
            this.settings = new TaxiHail.CompanySettings();
            $.when(this.settings.fetch({ data: { appSettingsType: 1 } })).then(this.ready);
        },

        index: function() {
            return new TaxiHail.ManageCompanySettingsView({
                model: this.settings
            });
        }
    });
}());