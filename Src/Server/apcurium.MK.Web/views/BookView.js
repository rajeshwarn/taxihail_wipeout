(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book'
        },
        
        initialize: function () {

            this.model.on('change', function(model, value) {
                
                // Enable the "Book Now!" button if model is valid
                if(this._addressIsValid(this.model.get('pickupAddress'))) {
                    this.$('[data-action=book]').removeClass('disabled');
                } else this.$('[data-action=book]').addClass('disabled');

            }, this);

           this.model.on('change:pickupAddress', function(model, value) {
                this.actualizeEstimate();
                this._pickupAddressView.model.set(value);
            }, this);

            this.model.on('change:dropOffAddress', function(model, value) {
                this.actualizeEstimate();
                this._dropOffAddressView.model.set(value);
            }, this);
            
             this.model.on('change:estimate', function(model, value){
                if(value.formattedPrice && value.formattedDistance) {
                    this.$('.estimate')
                        .removeClass('hidden')
                        .find('.fare')
                            .text(value.formattedPrice + ' (' + value.formattedDistance + ')');
                } else {
                    this.$('.estimate').addClass('hidden');
                }
             }, this);
            
           
        },
        
        

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            var pickupAddress = new Backbone.Model(),
                dropOffAddress = new Backbone.Model();

            this._pickupAddressView = new TaxiHail.AddressControlView({
                    model: pickupAddress,
                    locate: true
                });
            this._dropOffAddressView = new TaxiHail.AddressControlView({
                    model: dropOffAddress,
                    clear: true
            });
            
            

            this.$('.pickup-address-container').html(this._pickupAddressView.render().el);
            this.$('.drop-off-address-container').html(this._dropOffAddressView.render().el);
            
            
            this.model.set('isPickupActive', false);
            this.model.set('isDropOffActive', false);
            // Only one address picker can be open at once
           

            this._pickupAddressView.on('open', function(view){
                this._dropOffAddressView.close();
            }, this);

            this._dropOffAddressView.on('open', function(view){
                this._pickupAddressView.close();
            }, this);

            this._pickupAddressView.on('target', function (view, isActive) {
                this.model.set({
                    'isPickupActive': isActive,
                    'isDropOffActive': false
                });
                this._dropOffAddressView.toggleOff();
            }, this);

            this._dropOffAddressView.on('target', function (view, isActive) {
                this.model.set({
                    'isDropOffActive': isActive,
                    'isPickupActive': false
                });
                this._pickupAddressView.toggleOff();
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

            if(!this._addressIsValid(this.model.get('pickupAddress'))){
                this.$('[data-action=book]').addClass('disabled');
            }
            return this;
        },

        remove: function() {
            if(this._pickupAddressView) this._pickupAddressView.remove();
            if(this._dropOffAddressView) this._dropOffAddressView.remove();
            this.$el.remove();
            return this;
        },
        
        actualizeEstimate: function () {
            var pickup = this.model.get('pickupAddress'),
                dest = this.model.get('dropOffAddress');

            if (pickup && dest) {
                TaxiHail.directionInfo.getInfo(pickup.latitude, pickup.longitude, dest.latitude, dest.longitude)
                    .done(_.bind(function(result){

                        this.model.set({ 'estimate': result });

                    }, this));
            }
           
        },
                    
        book: function (e) {
            e.preventDefault();
            if(this._addressIsValid(this.model.get('pickupAddress'))) {
                TaxiHail.store.setItem("orderToBook", this.model.toJSON());
                TaxiHail.app.navigate('confirmationbook', { trigger:true });
            }
        },

        _addressIsValid: function(address){

            return address
                && address.fullAddress
                && address.latitude
                && address.longitude;

        }
    });

}());