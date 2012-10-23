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

            TaxiHail.app.navigate('rates', {trigger: true});

        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());