(function () {
    TaxiHail.OrderHistoryView = TaxiHail.TemplatedView.extend({
        events: {
        },

        initialize: function () {
            
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            this.$el.empty();
            if (this.collection.length) {
                this.collection.each(this.renderItem, this);
            } else {
                this.$el.append($('<li>').addClass('no-result').text(TaxiHail.localize('search.no-result')));
            }


            return this;
        },
        
        renderItem: function (model) {

            this.$el.append('Ibs Order Id : ' + model.get('iBSOrderId'));
        }


       
    });

}());
