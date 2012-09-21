(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=locate]': 'geolocalize',

        },
        
        initialize: function () {
            _.bindAll(this, "renderEstimateResults");
           this.model.on('change', this.render, this);

            this.model.on('change:pickupAddress', function(model, value) {
                this.actualizeEstimate();
            }, this);

            this.model.on('change:dropOffAddress', function(model, value) {
                this.actualizeEstimate();
            }, this);
            
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            var pickupAddress = new Backbone.Model(),
                dropOffAddress = new Backbone.Model(),
                pickupAddressView = new TaxiHail.AddressControlView({
                    model: pickupAddress
                }),
                dropOffAddressView = new TaxiHail.AddressControlView({
                    model: dropOffAddress
                });

            this.$('.pickup-address-container').html(pickupAddressView.render().el);
            this.$('.drop-off-address-container').html(dropOffAddressView.render().el);

            pickupAddressView.on('open', function(view){
                dropOffAddressView.close();
            }, this);

            dropOffAddressView.on('open', function(view){
                pickupAddressView.close();
            }, this);

            pickupAddress.on('change', function(model){
                this.model.set({
                    pickupAddress: model.toJSON()
                });
            }, this);

            dropOffAddress.on('change', function(model){
                this.model.set({
                    dropOffAddress: model.toJSON()
                });
            }, this);
            
            return this;
        },
        
        actualizeEstimate: function () {
            if (this.model.get('pickupAddress') && this.model.get('dropOffAddress')) {
                var pickup = this.model.get('pickupAddress');
                var dest = this.model.get('dropOffAddress');
                TaxiHail.directionInfo.getInfo(pickup['latitude'], pickup['longitude'], dest['latitude'], dest['longitude']).done(this.renderEstimateResults);
            }
           
        },
        
        renderEstimateResults: function (result) {

            this.model.set({
                'priceEstimate': result.formattedPrice,
                'distanceEstimate': result.formattedDistance
            });
            
            this.render();
           
        },
        
        geolocalize : function () {
            this.model.set('isLocating', true);
        },

               
        book: function (e) {
            e.preventDefault();
            TaxiHail.store.setItem("orderToBook", this.model.toJSON());
            TaxiHail.app.navigate('confirmationbook',{trigger:true});
        }
    });

}());