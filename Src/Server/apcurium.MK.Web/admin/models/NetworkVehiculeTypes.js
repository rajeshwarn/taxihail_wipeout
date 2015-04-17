(function () {

    TaxiHail.NetworkVehiculeTypes = Backbone.Collection.extend({
        url: TaxiHail.parameters.customerPortalUrl + '/api/customer/' + TaxiHail.parameters.applicationKey + '/marketVehicleTypes'
    });

}());