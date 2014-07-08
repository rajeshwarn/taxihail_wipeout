(function(){

    TaxiHail.VehicleTypeCollection = Backbone.Collection.extend({
        model: TaxiHail.VehicleType,
        url: TaxiHail.parameters.apiRoot + '/admin/vehicletypes'
    });
}());