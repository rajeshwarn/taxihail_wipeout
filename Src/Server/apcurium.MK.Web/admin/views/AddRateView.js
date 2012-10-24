(function(){
    

    var View = TaxiHail.AddRateView  = TaxiHail.TemplatedView.extend({

        render: function() {

            this.$el.html(this.renderTemplate());
            this.validate({
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