(function(){

    var Controller = TaxiHail.ExclusionsController = TaxiHail.Controller.extend({
        initialize: function() {

            this.referenceData = new TaxiHail.ReferenceData();
            this.settings = new TaxiHail.CompanySettings();
            
            $.when(this.referenceData.fetch( {
                data: {
                    withoutFiltering: true
                }}), this.settings.fetch({
                data: {
                    appSettingsType: 1
                }
            })).then(this.ready);
        },

        index: function() {
            return this.view = new TaxiHail.ManageExclusionsView({
                model: this.referenceData,
                settings: this.settings
            });
        }
    });


}());