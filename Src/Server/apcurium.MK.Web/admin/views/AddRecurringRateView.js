(function(){
    

    var View = TaxiHail.AddRecurringRateView  = TaxiHail.TemplatedView.extend({

        render: function() {

            var defaults = {
                flatRate: 0,
                pricePerPassenger: 0,
                distanceMultiplicator: 1,
                timeAdjustmentFactor: 1
            };

            this.$el.html(this.renderTemplate(defaults));
            this.validate({
                rules: {
                    flatRate: {
                        required: true,
                        min: 0
                    },
                    distanceMultiplicator: {
                        required: true,
                        min:0
                    },
                    timeAdjustmentFactor: {
                        required: true,
                        min:0
                    },
                    pricePerPassenger: {
                        required: true,
                        min: 0
                    },
                    daysOfTheWeek: {
                        required: true
                    }
                },
                submitHandler: this.save
            });

            return this;

        },

        save: function(form) {

            var serialized = this.serializeForm(form);
            serialized.daysOfTheWeek =  _([serialized.daysOfTheWeek])
                .flatten()
                .reduce(function(memo, num){ return memo + (1<<num); }, 0);

            this.collection.create(serialized);
            TaxiHail.app.navigate('rates', {trigger: true});

        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());