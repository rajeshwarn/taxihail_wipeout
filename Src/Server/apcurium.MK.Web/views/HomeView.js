(function () {

    TaxiHail.HomeView = TaxiHail.TemplatedView.extend({
        events: {
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            return this;
        }
    });

}());