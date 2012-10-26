(function(){
    
    TaxiHail.RatesController = TaxiHail.Controller.extend({

        initialize: function() {
            this.collection = new TaxiHail.RateCollection();
            $.when(this.collection.fetch()).then(this.ready);
        },

        index: function() {
            return this.view = new TaxiHail.ManageRatesView({
                collection: this.collection
            });
        },

        addRecurring: function() {
            return this.view = new TaxiHail.AddRecurringRateView({
                collection: this.collection,
                model: new TaxiHail.Rate()
            });
        },

        addDay: function() {
            return this.view = new TaxiHail.AddDayRateView({
                collection: this.collection,
                model: new TaxiHail.Rate()
            });
        },

        edit: function(id) {
            var model = this.collection.get(id);
            return this.view = new TaxiHail.EditRateView({
                collection: this.collection,
                model: model
            });
        }

    });

}());