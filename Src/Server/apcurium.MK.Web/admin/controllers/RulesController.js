(function () {

    TaxiHail.RulesController = TaxiHail.Controller.extend({

        initialize: function () {
            this.collection = new TaxiHail.RuleCollection();
            $.when(this.collection.fetch()).then(this.ready);
        },

        index: function () {
            return new TaxiHail.ManageRulesView({
                collection: this.collection
            });
        },

        addDefault: function () {
            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.recurring
                })
            });
        },

        addRecurring: function () {
            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.recurring
                })
            });
        },

        addDay: function () {
            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Tariff({
                    type: TaxiHail.Tariff.type.day
                })
            });
        },

        edit: function (id) {
            var model = this.collection.get(id);
            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: model
            });
        }

    });

}());