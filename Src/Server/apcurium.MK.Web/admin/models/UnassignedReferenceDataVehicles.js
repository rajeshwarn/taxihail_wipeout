(function () {

    TaxiHail.UnassignedReferenceDataVehicles = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/vehicletypes/unassignedreference'
    });

}());