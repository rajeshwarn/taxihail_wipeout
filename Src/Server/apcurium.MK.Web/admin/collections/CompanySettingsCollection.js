(function () {

    TaxiHail.CompanySettingsCollection = Backbone.Collection.extend({
        model: TaxiHail.CompanySetting,
        url: "../api/settings"
    });

}());