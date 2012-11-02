(function () {
    var canCancel = true;
    TaxiHail.BookingStatusView = TaxiHail.TemplatedView.extend({

        className: 'booking-status-view',

        events: {
            'click [data-action=cancel]': 'cancel'
        },

        initialize: function() {

            var status = this.model.getStatus();
            this.interval = window.setInterval(_.bind(function() {
                this.fetch();
            }, status), 5000);
            status.on('change:iBSStatusId', this.render, this);
            status.on('change:iBSStatusId', this.onStatusChanged, this);
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
            
            var status = this.model.getStatus();
            if (!status.isActive()) {
                this.$('[data-action=cancel]').addClass('disabled');
                canCancel = false;
            } else {
                canCancel = true;
            }

            this.$('[data-action=call]').popover({
                    title: this.localize("Call us at"),
                    content: TaxiHail.parameters.defaultPhoneNumber
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
            if (canCancel == true) {
                TaxiHail.confirm({
                    title: this.localize('Cancel Order'),
                    message: this.localize('modal.cancelOrder.message'),
                    cancelButton: this.localize('modal.cancelOrder.cancelButton')
                }).on('ok', function () {
                    this.model.cancel().done(function () {
                        // Redirect to Home
                        TaxiHail.app.navigate('', { trigger: true });
                    });
                }, this);
            }
            
        },

        onStatusChanged: function(model, status) {
            if(model.isCompleted()){

                // Prevent further updated
                window.clearInterval(this.interval);
                
                var message = this.localize('ThankYouNoteFormat').replace('{{ApplicationName}}', TaxiHail.parameters.applicationName);

                TaxiHail.confirm({
                    title: this.localize('Ride Complete'),
                    message: message,
                    confirmButton: this.localize('Send Receipt'),
                    autoHide: false
                }).on('cancel', function(view) {

                    TaxiHail.app.navigate('', {trigger: true});
                    view.hide();

                }, this).on('ok', function(view) {

                    // display "Sending..." for a brief delay before redirecting to home
                    var $button = view.$('[data-action=confirm]')
                        .data('loadingText', this.localize('Sending...'))
                        .button('loading');
                    
                    TaxiHail.postpone(function(){
                        this.model.sendReceipt()
                            .done(function(){
                                    view.hide();
                                    TaxiHail.app.navigate('', {trigger: true});
                                })
                            .fail(function(){
                                $button.text(TaxiHail.localize('Cannot Send Receipt'));
                            });
                        }, this).call();

                }, this);
            }
        }


    });
}());