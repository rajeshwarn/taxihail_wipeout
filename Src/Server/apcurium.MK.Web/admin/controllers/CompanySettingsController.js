(function(){

    var Controller = TaxiHail.CompanySettingsController = TaxiHail.Controller.extend({
        initialize: function() {
            this.settings = new TaxiHail.CompanySettings();
            $.when(this.settings.fetch({ data: { appSettingsType: 0 } })).then(this.ready);
        },

        index: function() {
            return this.view = new TaxiHail.ManageCompanySettingsView({
                model: this.settings
            });
        }
    });
}());