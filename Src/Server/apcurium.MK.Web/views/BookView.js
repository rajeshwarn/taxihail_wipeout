(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            return this;
        }
    });

}());