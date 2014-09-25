(function (TaxiHail) {

    var Model = TaxiHail.RideRatings;

    var Collection = TaxiHail.RideRatingCollection;

    var Controller = TaxiHail.RideRatingsController = TaxiHail.Controller.extend({
        initialize: function () {

            this.ratings = new Collection();
            $.when(this.ratings.fetch()).then(this.ready);
        },

        index: function () {
            return new TaxiHail.ManageRideRatingsView({
                collection: this.ratings
            });
        },

        add: function () {
            var model = new Model({
                isNew: true
            });

            return new TaxiHail.AddRideRatingView({
                model: model,
                collection: this.ratings
            }).on('cancel', function () {
                TaxiHail.app.navigate('ratings', { trigger: true });
            }, this);
        },

        edit: function (number) {

            var model = this.ratings.find(function(m) { return m.get('id') == number; });
            model.set('isNew', false);

            return new TaxiHail.AddRideRatingView({
                model: model,
                collection: this.ratings
            })
            .on('cancel', function () {
                TaxiHail.app.navigate('ratings', { trigger: true });
            }, this);
        }
    });

}(TaxiHail));