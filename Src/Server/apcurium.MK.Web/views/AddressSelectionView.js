(function() {

    TaxiHail.AddressSelectionView = TaxiHail.TemplatedView.extend({

        events: {
            'click .nav-tabs li>a': 'ontabclick'
        },

        initialize: function () {
            this.model.set('isLogged', TaxiHail.auth.isLogged());
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            this.tab.search.call(this);

            return this;
        },

        hide: function() {
            this.$el.addClass('hidden');
        },

        show: function() {
            this.$el.removeClass('hidden');
        },

        search: function(query) {
            TaxiHail.geocoder.geocode(query).done(_.bind(function(result) {
                this._searchResults && this._searchResults.reset(result.addresses);
            }, this));


        },
        
        selectTab: function($tab) {
            $tab.addClass('active').siblings().removeClass('active');
        },

        ontabclick: function(e) {
            e.preventDefault();

            var $tab = $(e.currentTarget).parent('li'),
                tabName = $tab.data().tab;

            this.selectTab($tab);
            this.tab[tabName].call(this);

        },

        tab: {
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

                this._searchResults = new TaxiHail.AddressCollection(),
                    view = new TaxiHail.AddressListView({
                        collection: this._searchResults
                    });

                this._searchResults.on('selected', function (model, collection) {

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