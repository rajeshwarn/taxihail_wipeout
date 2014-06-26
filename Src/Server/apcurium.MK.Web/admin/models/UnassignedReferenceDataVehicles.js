(function () {

    TaxiHail.UnassignedReferenceDataVehicles = Backbone.Collection.extend({
        url: TaxiHail.parameters.apiRoot + '/admin/vehicletypes/unassignedreference'
    });

}());