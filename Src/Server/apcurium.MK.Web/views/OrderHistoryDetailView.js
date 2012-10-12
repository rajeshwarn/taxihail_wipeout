(function () {
    TaxiHail.OrderHistoryDetailView = TaxiHail.TemplatedView.extend({
        events: {
            "click [data-action=rebook]": "rebook",
            "click [data-action=cancel]": "cancel",
            "click [data-action=remove]": "remove"
        },

        initialize: function () {

            this.model.on('change', this.render, this);
            
            this.model.id = this.model.get('id');
            this.model.getStatus().fetch({
                success: _.bind(function(model) {
                    var data = model.toJSON();
                    if (!data.iBSStatusDescription) {
                        data.iBSStatusDescription = TaxiHail.localize('Processing');
                    }
                    if (model.isActive()) {
                        this.model.set('canExecute', true);
                    } else {
                        this.model.set('canExecute', false);
                    }
                    this.model.set('orderStatus', data);
                },this)
            });

            
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            
            return this;
        },
        
        rebook : function () {
            this.model.saveLocal();
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
        }
    });

}());
