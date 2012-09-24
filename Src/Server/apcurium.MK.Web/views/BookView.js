(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=locate]': 'locate',

        },
        
        initialize: function () {
            _.bindAll(this, "renderEstimateResults");

            this.model.on('change', function(model, value) {
                
                // Enable the "Book Now!" button if model is valid
                if(this.model.isValid()) {
                    this.$('[data-action=book]').removeClass('disabled');
                } else this.$('[data-action=book]').addClass('disabled');

            }, this);

            this.model.on('change:pickupAddress', function(model, value) {
                this.actualizeEstimate();
            }, this);

            this.model.on('change:dropOffAddress', function(model, value) {
                this.actualizeEstimate();
            }, this);

            this.model.on('change:priceEstimate', function(model, value){
                this.$('.price-estimate').text(value);
            }, this);

            this.model.on('change:distanceEstimate', function(model, value){
                this.$('.distance-estimate').text(value);
            }, this);
            
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            var pickupAddress = new Backbone.Model(),
                dropOffAddress = new Backbone.Model();

            this._pickupAddressView = new TaxiHail.AddressControlView({
                    model: pickupAddress
                });
            this._dropOffAddressView = new TaxiHail.AddressControlView({
                    model: dropOffAddress
                });

            this.$('.pickup-address-container').html(this._pickupAddressView.render().el);
            this.$('.drop-off-address-container').html(this._dropOffAddressView.render().el);

            this._pickupAddressView.on('open', function(view){
                this._dropOffAddressView.close();
            }, this);

            this._dropOffAddressView.on('open', function(view){
                this._pickupAddressView.close();
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

            if(!this.model.isValid()){
                this.$('[data-action=book]').addClass('disabled');
            }
            
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
                       
        },
        
        locate : function () {
            TaxiHail.geolocation.getCurrentPosition()
                .done(_.bind(function(address){
                    this._pickupAddressView.model.set(address);
                }, this));
        },
               
        book: function (e) {
            e.preventDefault();
            if(this.model.isValid()) {
                TaxiHail.store.setItem("orderToBook", this.model.toJSON());
                TaxiHail.app.navigate('confirmationbook',{trigger:true});
            }
        }
    });

}());