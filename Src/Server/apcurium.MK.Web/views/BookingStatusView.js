(function () {

    TaxiHail.BookingStatusView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=cancel]': 'cancel'
        },

        initialize: function() {

            var status = this.model.getStatus();
            this.interval = window.setInterval(_.bind(status.fetch, status), 3000);
            status.on('change:iBSStatusId', this.render, this);

        },

        render: function() {

            this.$el.html(this.renderTemplate(this.model.getStatus().toJSON()));

            this.$('[data-action=call]').popover({
                title: 'title',
                content: 'content'
            });

            return this;
        },

        remove: function() {

            this.$el.remove();
            window.clearInterval(this.interval);

        },

        cancel: function(e) {
            e.preventDefault();
            this.model.cancel()
                .done(function(){
                    TaxiHail.app.navigate('', { trigger: true });
                });
        }


    });
}());