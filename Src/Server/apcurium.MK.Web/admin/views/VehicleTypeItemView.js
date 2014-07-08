(function() {

    TaxiHail.VehicleTypeItemView = TaxiHail.TemplatedView.extend({
        
        tagName: 'li',
        
        events: {
            'click [data-action=select-vehicletype]': 'selectVehicleType'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },
        
        selectVehicleType: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());