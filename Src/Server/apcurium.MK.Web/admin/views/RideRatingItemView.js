(function () {

    TaxiHail.RideRatingItemView = TaxiHail.TemplatedView.extend({

        tagName: 'li',

        events: {
            'click [data-action=select-rating]': 'selectRating'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },

        selectRating: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());