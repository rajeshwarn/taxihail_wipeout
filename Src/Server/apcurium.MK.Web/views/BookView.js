(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book'
        },
        
        initialize: function () {
            _.bindAll(this, "renderEstimateResults");
            //this.model.on('change', this.render, this);
            


            this.model.on('change:pickupAddress', function(model, value) {
                this.renderAddressControl('.pickup-address-container', new Backbone.Model(value), this.selectPickupAddress);
                this.actualizeEstimate();
            }, this);

            this.model.on('change:dropOffAddress', function(model, value) {
                this.renderAddressControl('.drop-off-address-container', new Backbone.Model(value), this.selectDropOffAddress);
                this.actualizeEstimate();
            }, this);
            
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            this.renderAddressControl('.pickup-address-container', new Backbone.Model(), this.selectPickupAddress);
            this.renderAddressControl('.drop-off-address-container', new Backbone.Model(), this.selectDropOffAddress);

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
            //TODO :
            //this.render();
        },

        // renderMap must be called after the view is added to the DOM
        renderMap: function() {
            var view = new TaxiHail.MapView({
                el: this.$('.map-container')[0],
                model: this.model
            }).render();

            return this;
        },

        renderAddressControl: function(selector, model, onselect) {

            var addressControlView = new TaxiHail.AddressControlView({
                model: model
            });
            addressControlView.on('select', onselect, this);

            this.$(selector).html(addressControlView.render().el);
        },
        
        selectPickupAddress: function (model) {
            this.showAddressList(function (model) {
                this.model.set({
                    pickupAddress: model.toJSON()
                });
            });

        },
        
        selectDropOffAddress: function(model) {
            this.showAddressList(function (model) {
                this.model.set({
                    dropOffAddress: model.toJSON()
                });
            });
            
        },
        
        showAddressList: function (onAddressSelected) {

            var view = new TaxiHail.AddressSelectionView();

            this.$('#pickup-drop-off-container').addClass('hidden-left');

            this.$('#address-list-container')
                .removeClass('hidden-right')
                .html(view.render().el);

            view.on('selected', function () {

                onAddressSelected.apply(this, arguments);
                this.$('#pickup-drop-off-container').removeClass('hidden-left');
                this.$('#address-list-container').addClass('hidden-right')
                
            }, this);
        },
        
        book: function (e) {
            e.preventDefault();
            TaxiHail.store.setItem("orderToBook", this.model.toJSON());
            TaxiHail.app.navigate('confirmationbook',{trigger:true});
        }
    });

}());