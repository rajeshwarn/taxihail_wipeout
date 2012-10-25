(function(){
    
    TaxiHail.RatesController = TaxiHail.Controller.extend({

        initialize: function() {
            this.collection = new TaxiHail.RateCollection();
            this.collection.fetch();
        },

        index: function() {
            return this.view = new TaxiHail.ManageRatesView({
                collection: this.collection
            });
        },

        addRecurring: function() {
            return this.view = new TaxiHail.AddRecurringRateView({
                collection: this.collection
            });
        },

        addDay: function() {
            return this.view = new TaxiHail.AddDayRateView({
                collection: this.collection
            });
        }

    });

}());