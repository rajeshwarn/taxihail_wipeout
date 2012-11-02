(function() {

    TaxiHail.CompanySetting = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/settings'
    });

}());