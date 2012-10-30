(function () {

    TaxiHail.CompanySettingsCollection = Backbone.Collection.extend({
        model: TaxiHail.CompanySetting,
        url: TaxiHail.parameters.apiRoot + "/settings"
    });

}());