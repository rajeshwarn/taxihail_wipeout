(function() {

    TaxiHail.AddressSelectionView = TaxiHail.TemplatedView.extend({

        events: {
            'click .nav-tabs li>a': 'selectTab'
        },

        initialize: function () {
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            this.show.favorites.call(this);

            return this;
        },
        
        selectTab: function(e) {
            e.preventDefault();

            var tabName = $(e.currentTarget).parent('li').data().tab;

            this.$('li').removeClass('active');
            $(e.currentTarget).parent('li').addClass('active');

            this.show[tabName].call(this);

        },

        show: {
            favorites: function() {

                var addresses = new TaxiHail.AddressCollection(),
                    view = new TaxiHail.AddressListView({
                        collection: addresses
                    });

                addresses.fetch({
                    url: 'api/account/addresses'
                });

                addresses.on('selected', function (model, collection) {
                    this.trigger('selected', model, collection);
                }, this);

                this.$('.tab-content').html(view.el);
            },

            search: function() {

                var addresses = new TaxiHail.AddressCollection(),
                    view = new TaxiHail.AddressSearchView({
                        collection: addresses
                    });

                addresses.on('selected', function (model, collection) {

                    if(!model.get('fullAddress'))
                    {
                        TaxiHail.geocoder.geocode(model.get('latitude'), model.get('longitude'))
                            .done(function(result){
                                if(result.addresses && result.addresses.length)
                                {
                                    model.set({
                                        fullAddress: result.addresses[0].fullAddress
                                    });
                                }

                            })
                            .always(_.bind(function(){
                                this.trigger('selected', model, collection);
                            }, this));
                    } else {
                        this.trigger('selected', model, collection);
                    } 
                    
                }, this);

                this.$('.tab-content').html(view.render().el);
            },

            places: function() {
                this.$('.tab-content').empty();
            }

        }

    });

}());