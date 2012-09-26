(function () {

    TaxiHail.BookingStatusView = TaxiHail.TemplatedView.extend({
        events: {


            
        },

        initialize: function() {

            this.interval = window.setInterval(_.bind(this.model.fetch, this.model), 3000);
            this.model.on('change:iBSStatusId', this.render, this);

        },

        render: function() {

            this.$el.html(this.renderTemplate(this.model.toJSON()));
            return this;
        },

        remove: function() {

            this.$el.remove();
            window.clearInterval(this.interval);

        }


    });
}());