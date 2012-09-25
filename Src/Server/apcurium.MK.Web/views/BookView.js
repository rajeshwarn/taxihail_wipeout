﻿(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=locate]': 'locate',
            

        },
        
        initialize: function () {
            this.model.set('isPickupBtnSelected', true);

            this.model.on('change', function(model, value) {
                
                // Enable the "Book Now!" button if model is valid
                if(this.model.isValid()) {
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
                this.$('.estimate').text(value.formattedPrice + ' (' + value.formattedDistance + ')');
             }, this);
            
           


            this.model.on('change:isPickupBtnSelected', function (model, value) {
                if (value == true) {
                    this._dropOffAddressView.$(".btn[data-action=toggleselect]").attr("class", "btn");
                    this._dropOffAddressView.isBtnSelected = false;
                    this._pickupAddressView.isBtnSelected = true;
                    
                } else {
                    this._pickupAddressView.$(".btn[data-action=toggleselect]").attr("class", "btn");
                    this._pickupAddressView.isBtnSelected = false;
                    this._dropOffAddressView.isBtnSelected = true;
                }
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
            
            this._pickupAddressView.$(".btn[data-action=toggleselect]").attr("class", "btn active");

            // Only one address picker can be open at once
           

            this._pickupAddressView.on('open', function(view){
                this._dropOffAddressView.close();
            }, this);

            this._dropOffAddressView.on('open', function(view){
                this._pickupAddressView.close();
            }, this);

            this._pickupAddressView.on('toggleselect', function (view) {
                    this.model.set('isPickupBtnSelected', true);
            }, this);

            this._dropOffAddressView.on('toggleselect', function (view) {
                    this.model.set('isPickupBtnSelected', false);
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

        locate : function () {
            TaxiHail.geolocation.getCurrentPosition()
                .done(_.bind(function (address) {
                   // if (this.model.isPickupBtnSelected) {
                        this._pickupAddressView.model.set(address);
                   // } else {
                    //    this._dropOffAddressView.model.set(address);
                    //}
                    
                    
                }, this));
        },
               
        book: function (e) {
            e.preventDefault();
            if(this.model.isValid()) {
                TaxiHail.store.setItem("orderToBook", this.model.toJSON());
                TaxiHail.app.navigate('confirmationbook', { trigger:true });
            }
        }
    });

}());