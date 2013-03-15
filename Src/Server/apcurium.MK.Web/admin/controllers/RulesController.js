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

        addDefaultDisable: function () {
            return new TaxiHail.EditRuleView({
                collection: this.collection,
                model: new TaxiHail.Rule({
                    type: TaxiHail.Rule.type.default,
                    category: TaxiHail.Rule.category.disableRule,
                    startTime: TaxiHail.date.toISO8601(new Date()),
                    endTime: TaxiHail.date.toISO8601(new Date())
                })
            });
        },

        addRecurringDisable: function () {
            return new TaxiHail.EditRuleView({
                collection: this.collection,
                model: new TaxiHail.Rule({
                    type: TaxiHail.Rule.type.recurring,
                    category: TaxiHail.Rule.category.disableRule,
                    startTime: TaxiHail.date.toISO8601(new Date()),
                    endTime: TaxiHail.date.toISO8601(new Date())
                })
            });
        },

        addDayDisable: function () {
            return new TaxiHail.EditTariffView({
                collection: this.collection,
                model: new TaxiHail.Rule({
                    type: TaxiHail.Rule.type.date,
                    category: TaxiHail.Rule.category.disableRule,
                    startTime: TaxiHail.date.toISO8601(new Date()),
                    endTime: TaxiHail.date.toISO8601(new Date())
                })
            });
        },

        addDefaultWarning: function () {
            return new TaxiHail.EditRuleView({
                collection: this.collection,
                model: new TaxiHail.Rule({
                    type: TaxiHail.Rule.type.default,
                    category: TaxiHail.Rule.category.warningRule,
                    startTime: TaxiHail.date.toISO8601(new Date()),
                    endTime: TaxiHail.date.toISO8601(new Date())
                })
            });
        },

        addRecurringWarning: function () {
            return new TaxiHail.EditRuleView({
                collection: this.collection,
                model: new TaxiHail.Rule({
                    type: TaxiHail.Rule.type.recurring,
                    category: TaxiHail.Rule.category.warningRule,
                    startTime: TaxiHail.date.toISO8601(new Date()),
                    endTime: TaxiHail.date.toISO8601(new Date())
                })
            });
        },

        addDayWarning: function () {
            return new TaxiHail.EditRuleView({
                collection: this.collection,
                model: new TaxiHail.Rule({
                    type: TaxiHail.Rule.type.date,
                    category: TaxiHail.Rule.category.warningRule,
                    startTime: TaxiHail.date.toISO8601(new Date()),
                    endTime: TaxiHail.date.toISO8601(new Date())
                })
            });
        },

        edit: function (id) {
            var model = this.collection.get(id);
            return new TaxiHail.EditRuleView({
                collection: this.collection,
                model: model
            });
        }

    });

}());