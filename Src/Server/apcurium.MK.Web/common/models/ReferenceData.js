(function () {

    TaxiHail.ReferenceData = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/referencedata'
    });

}());