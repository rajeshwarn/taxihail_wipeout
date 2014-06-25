(function(TaxiHail){

    var Model = TaxiHail.VehicleType;

    var Collection = TaxiHail.VehicleTypeCollection;

    var Controller = TaxiHail.VehicleTypeController = TaxiHail.Controller.extend({
        initialize: function() {

            this.vehicleTypes = new Collection();
            this.availableVehicles = new TaxiHail.UnassignedReferenceDataVehicles();

            $.when(this.vehicleTypes.fetch()).then(this.ready);
            $.when(this.availableVehicles.fetch()).then(this.ready);

        },

        index: function() {
            return new TaxiHail.ManageVehicleTypesView({
                collection: this.vehicleTypes
            });
        },

        add: function() {
            var model = new Model({
                isNew: true,
                availableVehicles: this.availableVehicles
            });

            return new TaxiHail.AddVehicleTypeView({
                model: model,
                collection: this.vehicleTypes
            }).on('cancel', function() {
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
            }, this);
        },

        edit: function(id) {

            var model = this.vehicleTypes.find(function (m) { return m.get('id') == id; });
            model.set('isNew', false);
            model.set('availableVehicles', this.availableVehicles);
            
            return new TaxiHail.AddVehicleTypeView({
                model: model,
                collection: this.vehicleTypes
            })
            .on('cancel', function() {
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
            }, this);

        }
    });

}(TaxiHail));