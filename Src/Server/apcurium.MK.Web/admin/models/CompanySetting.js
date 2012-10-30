(function() {

    TaxiHail.CompanySetting = Backbone.Model.extend({
        urlRoot: '../api/settings',
        idAttribute: "key"
    });

}());