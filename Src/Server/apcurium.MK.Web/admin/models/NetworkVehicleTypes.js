(function () {

    TaxiHail.NetworkVehicleTypes = Backbone.Collection.extend({
        url: TaxiHail.parameters.apiRoot + '/admin/vehicletypes/unassignednetworkvehicletype'
    });

}());