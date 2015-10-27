(function() {

    TaxiHail.ServiceTypeItemView = TaxiHail.TemplatedView.extend({
        
        tagName: 'li',
        
        events: {
            'click [data-action=select-servicetype]': 'selectServiceType'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },
        
        selectServiceType: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());