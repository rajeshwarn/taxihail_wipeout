(function(){

    var Controller = TaxiHail.PopularAddressesController  = TaxiHail.Controller.extend({

        initialize: function() {
            this.addresses = new TaxiHail.AddressCollection({
                url: '../api/admin/popularaddresses'
            });

            $.when(this.addresses.fetch()).then(this.ready);
        },

        index: function() {
            return new TaxiHail.ManagePopularAddressesView({
                collection: this.addresses
            });
        },

        add: function() {

            var model = new TaxiHail.Address({
                isNew: true
            });
            return new TaxiHail.AddPopularAddressView({
                model: this.model,
                collection: this.addresses,
                showPlaces: false
            }).on('cancel', function() {
                TaxiHail.app.navigate('addresses/popular', {trigger:true});
            }, this);

        },

        edit: function(id) {
            var model = this.addresses.get(id);
            model.set('isNew', false);
            
            return new TaxiHail.AddPopularAddressView({
                model: model,
                collection: this.addresses,
                showPlaces: false
            }).on('cancel', function() {
                TaxiHail.app.navigate('addresses/popular', {trigger:true});
            }, this);
        }

    });


}());