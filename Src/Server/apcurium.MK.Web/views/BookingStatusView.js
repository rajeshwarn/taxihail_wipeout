(function () {

    TaxiHail.BookingStatusView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=cancel]': 'cancel',
            'click [data-action=call]':   'call',
            'click [data-action=new]':    'new'
        },

        initialize: function() {

            var status = this.model.getStatus();
            this.interval = window.setInterval(_.bind(status.fetch, status), 3000);
            status.on('change:iBSStatusId', this.render, this);

        },

        render: function() {

            this.$el.html(this.renderTemplate(this.model.getStatus().toJSON()));
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
        },

        call: function(e) {
            e.preventDefault();
        },

        'new': function(e) {
            e.preventDefault();
            TaxiHail.app.navigate('', { trigger: true });

        }


    });
}());