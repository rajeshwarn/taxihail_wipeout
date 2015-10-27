(function(){

    TaxiHail.ServiceTypeCollection = Backbone.Collection.extend({
        model: TaxiHail.ServiceType,
        url: TaxiHail.parameters.apiRoot + '/admin/servicetypes'
    });
}());