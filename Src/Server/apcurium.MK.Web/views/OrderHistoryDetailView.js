(function () {
    TaxiHail.OrderHistoryDetailView = TaxiHail.TemplatedView.extend({
        events: {
            "click [data-action=rebook]": "rebook",
            "click [data-action=cancel]": "cancel",
            "click [data-action=remove]": "remove",
            "click [data-action=view-status]": "viewStatus",
            "click [data-action=send-receipt]": "sendReceipt"
        },

        initialize: function () {

            this.model.on('change', this.render, this);
            
            this.model.id = this.model.get('id');
            this.model.getStatus().fetch({
                success: _.bind(function(model) {
                    var data = model.toJSON();
                    if (!model.get('ibsStatusDescription')) {
                        model.set('ibsStatusDescription', TaxiHail.localize('Processing'));
                    }
                    this.render();
                },this)
            });

            
        },

        render: function () {

            var data = this.model.toJSON(),
                status = this.model.getStatus();
            
            _.extend(data, {
                orderStatus:     status.toJSON(),
                canCancel: status.isActive(),
                isActive: status.isActive(),
                canDelete:      !status.isActive(),
                canPrintReceipt: status.canSendReceipt()
            });

            this.$el.html(this.renderTemplate(data));
            
            return this;
        },
        
        rebook : function () {
            var attrs = _.pick(this.model.attributes, "dropOffAddress", "pickupAddress", "pickupDate", "settings");
            var newOrder = new TaxiHail.Order(attrs);
            newOrder.saveLocal();
            TaxiHail.app.navigate('confirmationbook', { trigger: true });
        },
        
        cancel : function () {
            if (this.model.getStatus().isActive()) {
                TaxiHail.confirm({
                    title: this.localize('Cancel Order'),
                    message: this.localize('modal.cancelOrder.message'),
                    cancelButton: this.localize('modal.cancelOrder.cancelButton')
                }).on('ok', function () {
                    this.model.cancel();
                    this.model.trigger('cancel', this);
                }, this);
            }
        },
        
        remove: function () {
            if (!this.model.getStatus().isActive()) {
                TaxiHail.confirm({
                    title: this.localize('Remove Order'),
                    message: this.localize('modal.removeOrder.message')
                }).on('ok', function () {
                    this.model.destroy();
                }, this);
            }
        },
        viewStatus: function (form) {
            var lang = TaxiHail.getClientLanguage();
            this.model.set('ClientLanguageCode', lang);
            this.model.set('FromWebApp', true);
            this.model.saveLocal();
            var model = this.model;

            TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
            
        },
        sendReceipt: function() {
            if (this.model.getStatus().isCompleted()) {
                
                var $button = this.$('[data-action=send-receipt]');
                $button.button('loading');
                
                TaxiHail.postpone(function() {
                    this.model.sendReceipt()
                        .done(_.bind(function(){
                            $button.addClass('btn-success').text(this.localize('Receipt Sent'));
                        }, this))
                        .fail(_.bind(function(){
                            $button.text(this.localize('Cannot Send Receipt'));
                        }, this));
                }, this)();
            }
        }
    });

}());
