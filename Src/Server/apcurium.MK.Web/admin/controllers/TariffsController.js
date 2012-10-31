(function(){
    
    TaxiHail.TariffsController = TaxiHail.Controller.extend({

        initialize: function() {
            this.collection = new TaxiHail.TariffCollection();
            $.when(this.collection.fetch()).then(this.ready);
        },

        index: function() {
            return this.view = new TaxiHail.ManageTariffsView({
                collection: this.collection
            });
        },

        addRecurring: function() {
            return this.view = new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.recurring
                })
            });
        },

        addDay: function() {
            return this.view = new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.day
                })
            });
        },

        edit: function(id) {
            var model = this.collection.get(id);
            return this.view = new TaxiHail.EditTariffView({
                collection: this.collection,
                model: model
            });
        }

    });

}());