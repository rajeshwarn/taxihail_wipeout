(function() {

    TaxiHail.AddressItemView = TaxiHail.TemplatedView.extend({
        
        tagName: 'li',
        
        events: {
            'click [data-action=select-address]': 'selectAddress'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            if(!this.model.get('friendlyName') || !this.model.get('fullAddress')) {
                this.$el.addClass('single-line');
            }

            return this;
        },
        
        selectAddress: function (e) {
            e.preventDefault();
            if (this.model.get('addressType') == "craftyclicks") {
                this.processCraftyClickAddress(this.model);
            } else {
                this.model.trigger('selected', this.model, this.model.collection);
            }
        },

        processCraftyClickAddress: function(model) {
            var address = model.get('fullAddress');
            var lat = model.get('latitude');
            var lng = model.get('longitude');

            TaxiHail.geocoder.search(address, lat, lng)
                .done(function (address) {
                    model.set(address[0]);
                    model.trigger('selected', model, model.collection);
                }, this);
        }

    });

}());