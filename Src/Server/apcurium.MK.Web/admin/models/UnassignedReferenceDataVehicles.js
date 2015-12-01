(function () {

    TaxiHail.UnassignedReferenceDataVehicles = Backbone.Collection.extend({
        initialize: function (models, options) {},
        url: TaxiHail.parameters.apiRoot + '/admin/vehicletypes/unassignedreference/'
});

}());