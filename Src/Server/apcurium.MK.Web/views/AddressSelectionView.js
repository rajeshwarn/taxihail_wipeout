(function() {

    TaxiHail.AddressSelectionView = TaxiHail.TemplatedView.extend({

        className: 'tabs-below',

        events: {
            'click .nav-tabs li>a': 'ontabclick'
        },

        initialize: function () {
            _.bindAll(this, 'hide', 'ondownarrow', 'onuparrow', 'onenter');
            TaxiHail.auth.on('change init', this.render, this);
            $(document).bind('keydown', 'down', this.ondownarrow);
            $(document).bind('keydown', 'up', this.onuparrow);
            $(document).bind('keydown', 'return', this.onenter);
        },

        render: function () {

            var data = _.extend(this.model.toJSON(), {
                isLoggedIn: TaxiHail.auth.isLoggedIn()
            });

            this.$el.html(this.renderTemplate(data));

            this.tab.search.call(this);

            return this;
        },

        remove: function() {
            TaxiHail.auth.off(null, null, this);
            $(document).unbind('keydown', this.ondownarrow);
            $(document).unbind('keydown', this.onuparrow);
            $(document).unbind('keydown', this.onenter);

            this.$el.remove();
            return this;
        },

        hide: function() {
            this.$el.addClass('hidden');
        },

        show: function() {
            this.$el.removeClass('hidden');
        },

        search: function(query) {

            // Ensure Search tab is selected
            var $tab = this.$('[data-tab=search]');
            if(!$tab.is('.active')) {
                this.selectTab(this.$('[data-tab=search]'));
                this.tab.search.call(this);
            }
            
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

        ondownarrow: function(e) {
            var $addresses = this.$el.find('[data-action=select-address]'),
                $active = $addresses.filter('.active');
            if(!$active.length) {
                $addresses.first().addClass('active');
            } else {
                $active.removeClass('active').parent('li').next().find('[data-action=select-address]').addClass('active');
            }
        },

        onuparrow: function(e) {
            var $addresses = this.$el.find('[data-action=select-address]'),
                $active = $addresses.filter('.active');
            if(!$active.length) {
                $addresses.last().addClass('active');
            } else {
                $active.removeClass('active').parent('li').prev().find('[data-action=select-address]').addClass('active');
            }
        },

        onenter: function(e) {
            var $addresses = this.$el.find('[data-action=select-address]'),
                $active = $addresses.filter('.active');
            if($active.length) {
                $active.click();
            }
        },

        tab: {
            favorites: function() {

                var addresses = new TaxiHail.AddressCollection(),
                    view = new TaxiHail.FavoritesAndHistoryListView({
                        collection: addresses
                    });

                var favorites = new TaxiHail.AddressCollection();
                var history = new TaxiHail.AddressCollection();
                favorites.fetch({
                    url: 'api/account/addresses',
                    success: function(collection, resp) {
                        history.fetch({
                            url: 'api/account/addresses/history',
                            success: function(collection, resp) {
                                addresses.reset(favorites.models.concat(history.models));
                            }
                        });
                    }
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

            places: function () {

                var opts = {
                    lines: 11, // The number of lines to draw
                    length: 3, // The length of each line
                    width: 3, // The line thickness
                    radius: 6, // The radius of the inner circle
                    corners: 1, // Corner roundness (0..1)
                    rotate: 0, // The rotation offset
                    speed: 1, // Rounds per second
                    trail: 60, // Afterglow percentage
                    shadow: false, // Whether to render a shadow
                    hwaccel: false, // Whether to use hardware acceleration
                    className: 'spinner-address', // The CSS class to assign to the spinner
                    zIndex: 2e9 // The z-index (defaults to 2000000000)
                };
            
               
                var spinner = new Spinner(opts).spin();
                this.$('.tab-content').html(spinner.el);
                
               TaxiHail.geolocation.getCurrentPosition()
                .done(TaxiHail.postpone(_.bind(function (address) {
                    this.fetchPlaces(address.latitude, address.longitude);
                }, this)))
                .fail(_.bind(function () {
                    this.fetchPlaces(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude);
                }, this));
            }
        },
        
        fetchPlaces: function(latitude, longitude) {
                $.get('api/places', {
                    lat: latitude,
                    lng: longitude,
                    format: 'json'
                }, _.bind(function (result) {
                    
                    this._searchResults = new TaxiHail.AddressCollection(),
                        view = new TaxiHail.AddressListView({
                            collection: this._searchResults
                        });
                    this._searchResults.reset(result);
                    
                    this._searchResults.on('selected', function (model, collection) {
                        this.trigger('selected', model, collection);
                    }, this);
                    
                    this.$('.tab-content').html(view.render().el);
            }, this));
        }

    });

}());