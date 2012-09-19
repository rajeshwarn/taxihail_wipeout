(function () {

    TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },
        
        book: function (e) {
        e.preventDefault();

        this.model.save();
    }
    });

}());


