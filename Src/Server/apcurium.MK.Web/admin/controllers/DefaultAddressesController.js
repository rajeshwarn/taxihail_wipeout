(function(TaxiHail){

    var Controller = TaxiHail.DefaultAddressesController = TaxiHail.Controller.extend({
        initialize: function() {

            this.addresses = new TaxiHail.CompanyDefaultAddressCollection();
        
            $.when(this.addresses.fetch()).then(this.ready);

        },

        index: function() {
            return new TaxiHail.ManageDefaultAddressesView({
                collection: this.addresses
            });
        },

        add: function() {
            var model = new TaxiHail.CompanyDefaultAddress({
                isNew: true
            });

            return new TaxiHail.AddFavoriteView({
                model: model,
                collection: this.addresses,
                showPlaces: false
            }).on('cancel', function() {
                TaxiHail.app.navigate( '' /* Manage Default Addresses */, {trigger: true} );
            }, this);
        },

        edit: function(id) {

            var model = this.addresses.get(id);
            model.set('isNew', false);
            
            return new TaxiHail.AddFavoriteView({
                model: model,
                collection: this.addresses,
                showPlaces: false
            })
            .on('cancel', function() {
                TaxiHail.app.navigate( '' /* Manage Default Addresses */, {trigger: true} );
            }, this);

        }
    });

}(TaxiHail));