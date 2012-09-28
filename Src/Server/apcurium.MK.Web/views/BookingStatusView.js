(function () {

    TaxiHail.BookingStatusView = TaxiHail.TemplatedView.extend({

        className: 'booking-status-view',

        events: {
            'click [data-action=cancel]': 'cancel'
        },

        initialize: function() {

            var status = this.model.getStatus();
            this.interval = window.setInterval(_.bind(status.fetch, status), 5000);
            status.on('change:iBSStatusId', this.render, this);
        },

        render: function() {

            // Close popover if it is open
            // Otherwise it will stay there forever
            this.$('[data-action=call]').popover('hide');
            var data = this.model.getStatus().toJSON();
            if(!data.iBSStatusDescription)
            {
                data.iBSStatusDescription = this.localize('Processing');
            }

            this.$el.html(this.renderTemplate(data));

            this.$('[data-action=call]').popover({
                    title:"Call me maybe",
                    content:"514 692 6813"
                });

            return this;
        },

        remove: function() {

            // Close popover if it is open
            // Otherwise it will stay there forever
            this.$('[data-action=call]').popover('hide');

            this.$el.remove();

            // Stop polling for Order Status updates
            window.clearInterval(this.interval);

        },

        cancel: function(e) {
            e.preventDefault();
            this.model.cancel()
                .done(function(){
                    // Redirect to Home
                    TaxiHail.app.navigate('', { trigger: true });
                });
        }


    });
}());