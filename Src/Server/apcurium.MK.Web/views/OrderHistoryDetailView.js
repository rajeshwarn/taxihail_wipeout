(function () {
    TaxiHail.OrderHistoryDetailView = TaxiHail.TemplatedView.extend({
        events: {
            "click [data-action=rebook]": "rebook",
            "click [data-action=cancel]": "cancel",
            "click [data-action=remove]": "remove",
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
            if (this.model.getStatus().isActive() == true) {
                TaxiHail.confirm(this.localize('Cancel Order'),
                this.localize('modal.cancel.message'),
                _.bind(function () {
                    this.model.cancel();
                    this.model.trigger('cancel', this);
                }, this));
            }
        },
        
        remove: function () {
            if (this.model.getStatus().isActive() == false) {
                TaxiHail.confirm(this.localize('Remove Order'),
                this.localize('modal.remove.message'),
                _.bind(function () {
                    this.model.destroy();
                }, this));
            }
        }
    });

}());
