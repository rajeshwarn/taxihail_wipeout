(function () {

    TaxiHail.ManageRideRatingsView = TaxiHail.TemplatedView.extend({

        initialize: function () {
            this.collection.on('selected', this.edit, this);
        },

        render: function () {

            this.$el.html(this.renderTemplate());

            var $ul = this.$('ul');
            var items = this.collection.reduce(function (memo, model) {
                isNew: model.isNew(),
                memo.push(new TaxiHail.RideRatingItemView({
                    model: model
                }).render().el);
                return memo;
            }, []);

            $ul.first().append(items);

            var $add = $('<a>')
                .attr('href', '#ratings/add')
                .addClass('new')
                .text(this.localize('rideRatings.add-new'));

            $ul.first().append($('<li>').append($add));

            return this;
        },

        edit: function (model) {
            if (!model.isNew()) {
                TaxiHail.app.navigate('ratings/edit/' + model.get('id'), { trigger: true });
            }
        }
    });
}());