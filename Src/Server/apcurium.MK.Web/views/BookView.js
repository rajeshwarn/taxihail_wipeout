(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book'
        },
        
        initialize: function () {

            this.model.on('change:pickupAddress', function(model, value) {
                this.renderAddressControl('.pickup-address-container', new Backbone.Model(value), this.selectPickupAddress);
            }, this);

            this.model.on('change:dropOffAddress', function(model, value) {
                this.renderAddressControl('.drop-off-address-container', new Backbone.Model(value), this.selectDropOffAddress);
            }, this);
            
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            this.renderAddressControl('.pickup-address-container', new Backbone.Model(), this.selectPickupAddress);
            this.renderAddressControl('.drop-off-address-container', new Backbone.Model(), this.selectDropOffAddress);

           /* var view = new TaxiHail.MapView({
                el: this.$('#map-container')[0],
                model: this.model
            });*/

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