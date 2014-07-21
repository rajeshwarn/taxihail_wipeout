(function(){
    
    TaxiHail.TariffsController = TaxiHail.Controller.extend({

        initialize: function() {
            this.collection = new TaxiHail.TariffCollection();
            this.vehicleTypes = new TaxiHail.VehicleTypeCollection();

            $.when(
               this.collection.fetch(),
               this.vehicleTypes.fetch()
            ).then(this.ready);
        },

        index: function() {
            return new TaxiHail.ManageTariffsView({
                collection: this.collection
            });
        },

        addRecurring: function () {

            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.recurring
                }),
                availableVehicleTypes: this.vehicleTypes
            });
        },

        addDay: function () {

            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.day
                }),
                availableVehicleTypes: this.vehicleTypes
            });
        },

        addVehicle: function () {

            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.vehicleDefault
                }),
                availableVehicleTypes: this.vehicleTypes
            });
        },

        edit: function(id) {
            var model = this.collection.get(id);

            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: model,
                availableVehicleTypes: this.vehicleTypes
            });
        }

    });

}());