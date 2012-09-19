// This control is used for Pickup and DropOff address in the Book screen
(function(){

    TaxiHail.AddressControlView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-action=select-address]': 'selectAddress'
        },


        render: function() {

            this.$el.html(this.renderTemplate(this.model.toJSON()));
            return this;
        },

        selectAddress: function(e) {
            e.preventDefault();

            this.trigger('select', this.model);

        }




    });

}());