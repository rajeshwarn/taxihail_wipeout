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

        add: function() {
            return this.view = new TaxiHail.AddRateView({
                collection: this.collection
            });
        }

    });

}());