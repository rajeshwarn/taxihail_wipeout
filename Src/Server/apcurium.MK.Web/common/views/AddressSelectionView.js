(function() {

    TaxiHail.AddressSelectionView = TaxiHail.TemplatedView.extend({

        className: 'tabs-below',

        options: {
            showFavorites: true,
            showPlaces: true
        },

        events: {
            'click .nav-tabs li>a': 'ontabclick'
        },

        initialize: function () {
            _.bindAll(this, 'hide', 'ondownarrow', 'onuparrow', 'onenter');
            TaxiHail.auth.on('change', this.render, this);
            $(document).bind('keydown', 'down', this.ondownarrow);
            $(document).bind('keydown', 'up', this.onuparrow);
            $(document).bind('keydown', 'return', this.onenter);

            this.spinnerOptions = {
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
        },

        render: function () {

            var data = _.extend(this.model.toJSON(), {
                showFavorites: this.options.showFavorites && TaxiHail.auth.isLoggedIn(),
                showPlaces: this.options.showPlaces
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
            if (TaxiHail.parameters.craftyclicksapikey) {

                TaxiHail.craftyclicks.getCraftyClicksAdresses(query).done(_.bind(function (result) {
                    if (result.error_code) {
                        this.searchWithGoogleGeocoder(query);
                    } else {
                        this._searchResults && this._searchResults.reset(TaxiHail.craftyclicks.toAddress((result)));
                    }
                }, this));
            }
            else {
                this.searchWithGoogleGeocoder(query);
            }

        },

        searchWithGoogleGeocoder: function(query) {
            TaxiHail.geocoder.search(query).done(_.bind(function (result) {
                this._searchResults && this._searchResults.reset(result);
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

                var spinner = new Spinner(this.spinnerOptions).spin();
                this.$('.tab-content').html(spinner.el);
                
                var addresses = new TaxiHail.AddressCollection(),
                    view = new TaxiHail.FavoritesAndHistoryListView({
                        collection: addresses
                    });

                var favorites = new TaxiHail.AddressCollection();
                var history = new TaxiHail.AddressCollection();
                favorites.fetch({
                    url: 'api/account/addresses',
                    success: _.bind(function(collection, resp) {
                        history.fetch({
                            url: 'api/account/addresses/history',
                            success: _.bind(function(collection, resp) {
                                addresses.reset(favorites.models.concat(history.models));
                                this.$('.tab-content').html(view.el);
                            }, this)
                        });
                    }, this)
                });
                
                addresses.on('selected', function (model, collection) {
                    this.trigger('selected', model, collection);
                }, this);
            },

            search: function() {

                this._searchResults = new Backbone.Collection(),
                    view = new TaxiHail.AddressListView({
                        collection: this._searchResults
                    });

                this._searchResults.on('selected', function (model, collection) {

                    // Fetch place details in case this is a Google Places result
                    this.fetchPlaceDetails(model)
                        .always(_.bind(function(){
                            this.trigger('selected', model, collection);
                        }, this));

                }, this);

                this.$('.tab-content').html(view.render().el);
            },

            places: function () {
               
                var spinner = new Spinner(this.spinnerOptions).spin();
                this.$('.tab-content').html(spinner.el);
               
                if (TaxiHail.geolocation.isActive) {
                    TaxiHail.geolocation.getCurrentPosition()
                     .done(TaxiHail.postpone(_.bind(function (coords) {
                         this.fetchPlaces(coords.latitude, coords.longitude);
                     }, this)))
                     .fail(_.bind(function () {
                         this.fetchPlaces(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude);
                     }, this));
                }
                else {
                    
                        this.fetchPlaces(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude);
                    
                }
            }
        },
        
        fetchPlaces: function(lat, lng) {
            TaxiHail.places.search(lat, lng)
                .done(_.bind(function (result) {
                
                    this._searchResults = new Backbone.Collection(),
                        view = new TaxiHail.AddressListView({
                            collection: this._searchResults
                        });
                    this._searchResults.reset(result);
                    
                    this._searchResults.on('selected', function (model, collection) {
                        this.fetchPlaceDetails(model)
                            .always(_.bind(function(){
                                this.trigger('selected', model, collection);
                            }, this));

                    }, this);
                    
                    this.$('.tab-content').html(view.render().el);
                
                }, this));
        },

        fetchPlaceDetails: function(model) {
            var placeId = model && model.get('placeId');
            var placeName = model && model.get('friendlyName');
            if (placeId) {
                return TaxiHail.places.getPlaceDetails(placeId, placeName)
                        .done(function(result){
                            model.set(result);
                        });
            }
            // Return resolved promise in case the call is not valid
            return $.when();
        }
    });
}());