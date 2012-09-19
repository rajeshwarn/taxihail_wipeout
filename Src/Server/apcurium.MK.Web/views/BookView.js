(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=select-pickup-address]': 'selectPickupAddress',
            'click [data-action=select-drop-off-address]': 'selectDropOffAddress',
            'click [data-action=book]': 'book'
        },
        
        initialize: function () {

            this.model.on('change', this.render, this);
            
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            var view = new TaxiHail.MapView({
                el: this.$('#map-container')[0],
                model: this.model
            });

            return this;
        },
        
        selectPickupAddress: function (e) {
            e.preventDefault();

            this.showAddressList(function (model) {
                this.model.set({
                    pickupAddress: model.toJSON()
                });
            });

        },
        
        selectDropOffAddress: function(e) {
            e.preventDefault();

            this.showAddressList(function (model) {
                this.model.set({
                    dropOffAddress: model.toJSON()
                });
            });
            
        },
        
        showAddressList: function (onAddressSelected) {
            var addresses = new TaxiHail.AddressCollection();

            var view = new TaxiHail.AddressSelectionView({
                collection: addresses
            });

            this.$('#address-list-container').html(view.render().el);

            addresses.fetch({
                url: 'api/account/addresses'
            });

            addresses.on('selected', function () {

                onAddressSelected.apply(this, arguments);
                view.remove();
                
            }, this);
        },
        
        book: function (e) {
            e.preventDefault();
            TaxiHail.store.setItem("orderToBook", this.model.toJSON());
            TaxiHail.app.navigate('confirmationbook',{trigger:true});
        }
    });

}());