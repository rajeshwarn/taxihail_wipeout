(function () {

    TaxiHail.BookingConfirmedView = TaxiHail.TemplatedView.extend({
        events: {
            
        },

        render: function() {

            this.$el.html(this.renderTemplate(this.model.toJSON()));


            return this;
        }
    });
}());