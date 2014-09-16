(function () {
    TaxiHail.OrderHistoryView = TaxiHail.TemplatedView.extend({
        events: {
        },

        initialize: function () {
            this.collection.on('selected', function (model, collection) {
                var detailsView = new TaxiHail.OrderHistoryDetailView({
                    model: model
                });
                this.$el.html(detailsView.render().el);
            }, this);
            this.collection.on('destroy cancel', this.render, this);
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            //this.$el.empty();
            if (this.collection.length) {
                this.collection.each(this.renderItem, this);
            } else {
                this.$el.append($('<div>').addClass('no-result').text(TaxiHail.localize('order.no-result')));
            }


            return this;
        },
        
        renderItem: function (model) {
            
            var view = new TaxiHail.OrderItemView({
                model: model
            });
            this.$('ul').append(view.el);
            view.render();
        }
    });

}());
