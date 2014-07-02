(function(TaxiHail){

    var Model = TaxiHail.VehicleType;

    var Collection = TaxiHail.VehicleTypeCollection;

    var Controller = TaxiHail.VehicleTypeController = TaxiHail.Controller.extend({
        initialize: function() {
            this.vehicleTypes = new Collection();
            this.availableVehicles = new TaxiHail.UnassignedReferenceDataVehicles();

            $.when(this.vehicleTypes.fetch(), this.availableVehicles.fetch()).then(this.ready);

            this.availableVehicles.on('reset', this.render, this);
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

            var model = new Model({
                isNew: true
            });

            return new TaxiHail.AddVehicleTypeView({
                model: model,
                collection: this.vehicleTypes,
                availableVehicles: this.availableVehicles
            }).on('cancel', function() {
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
            }, this);
        },

        edit: function(id) {
            var model = this.vehicleTypes.find(function (m) { return m.get('id') == id; });
            model.set('isNew', false);

            this.availableVehicles.fetch({ data: { vehicleBeingEdited: model.get('referenceDataVehicleId') } });

            return new TaxiHail.AddVehicleTypeView({
                model: model,
                collection: this.vehicleTypes,
                availableVehicles: this.availableVehicles
            })
            .on('cancel', function() {
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
            }, this);

        }
    });

}(TaxiHail));