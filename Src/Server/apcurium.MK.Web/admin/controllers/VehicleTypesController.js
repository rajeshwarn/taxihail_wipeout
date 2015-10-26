(function(TaxiHail){

    var Model = TaxiHail.VehicleType;

    var Collection = TaxiHail.VehicleTypeCollection;

    var Controller = TaxiHail.VehicleTypeController = TaxiHail.Controller.extend({
        initialize: function() {
            this.vehicleTypes = new Collection();
            
            $.when(this.vehicleTypes.fetch()).then(this.ready);
        },

        index: function () {
            return new TaxiHail.ManageVehicleTypesView({
                collection: this.vehicleTypes
            });
        },

        add: function () {
            if (this.vehicleTypes.length >= 4) {
                alert(TaxiHail.localize('error.vehicleTypesLimitReached'));
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
            }

            var availableVehicles = new TaxiHail.UnassignedReferenceDataVehicles();

            var networkVehicleTypes = new TaxiHail.NetworkVehicleTypes();

            var serviceTypes = TaxiHail.ServiceTypesEnum();

            var view = new TaxiHail.AddVehicleTypeView({
                model: new Model(),
                collection: this.vehicleTypes,
                availableVehicles: availableVehicles,
                networkVehicleTypes: networkVehicleTypes,
                serviceTypes: serviceTypes
            }).on('cancel', function() {
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
            }, this);

            availableVehicles.on('reset', view.render, view);
            availableVehicles.fetch();

            networkVehicleTypes.on('reset', view.render, view);
            networkVehicleTypes.fetch();

            return view;
        },

        edit: function(id) {
            var model = this.vehicleTypes.find(function (m) { return m.get('id') == id; });

            var availableVehicles = new TaxiHail.UnassignedReferenceDataVehicles();

            var networkVehicleTypes = new TaxiHail.NetworkVehicleTypes();

            var serviceTypes = new TaxiHail.ServiceTypes();

            var view = new TaxiHail.AddVehicleTypeView({
                model: model,
                collection: this.vehicleTypes,
                availableVehicles: availableVehicles,
                networkVehicleTypes: networkVehicleTypes,
                serviceTypes: ['Taxi', 'Luxury'] //serviceTypes
            })
            .on('cancel', function() {
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
            }, this);

            availableVehicles.on('reset', view.render, view);
            availableVehicles.fetch({ data: { vehicleBeingEdited: model.get('referenceDataVehicleId') } });

            networkVehicleTypes.on('reset', view.render, view);
            networkVehicleTypes.fetch({ data: { networkVehicleId: model.get('referenceNetworkVehicleTypeId') } });

            return view;
        }
    });

}(TaxiHail));