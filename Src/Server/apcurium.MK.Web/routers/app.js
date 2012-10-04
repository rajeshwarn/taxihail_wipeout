(function () {
    var currentView,
        renderView = function(ctor, model) {
            // Call remove on current view
            // in case it was overriden with custom logic
            if(currentView && _.isFunction(currentView.remove)) {
                currentView.remove();
            }
            
            currentView = new ctor({
                model: model
            }).render();

            $('#main').html(currentView.el);

            return currentView;

        },
        mapView;

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            "": "book",   // #
            "later": "later",
            "confirmationbook": "confirmationbook",
            "login/:url": "login", // #login
            "login": "login",
            "signup": "signup", // #signup
            "signupconfirmation": "signupconfirmation",
            "status/:id": "status",
            "useraccount": "useraccount",
            "resetpassword": "resetpassword"
        },

        initialize: function () {

            var expire = new Date();
            expire.setTime(expire.getTime() + 3600000 * 24 * 365);
            document.cookie = "ss-opt=perm" + ";expires=" + expire.toGMTString();
            
            TaxiHail.auth.initialize(function(isloggedIn) {
                if(isloggedIn) {
                    // Check if an order exists
                    // If order is not saved, go to confirmation
                    // If order is saved and status is active, go to status
                    var order = TaxiHail.orderService.getCurrentOrder();
                    if(order) {
                        if(order.isNew()){
                            this.navigate('confirmationbook', { trigger: true });
                        }
                        else {
                            order.getStatus().fetch({
                                success: _.bind(function(model, resp) {
                                    if(model.isActive()){
                                        this.navigate('status/' + order.id , { trigger: true });
                                    }
                                }, this)
                            });
                        }
                    } else {
                        this.navigate('', { trigger: true });
                    }
                }
            }, this);

            TaxiHail.auth.on('change', function(isloggedIn, urlToRedirect) {
                if (isloggedIn) {
                    TaxiHail.auth.account.fetch();
                    if (urlToRedirect) {
                        this.navigate(urlToRedirect, { trigger: true });

                    } else {
                        
                        this.navigate('', { trigger: true });
                    }
                } else {
                    this.navigate('', { trigger: true });
                }
            }, this);

            mapView = new TaxiHail.MapView({
                el: $('.map-zone')[0],
                model: new TaxiHail.Order()
            }).render();
            
            $('.login-status-zone').html(new TaxiHail.LoginStatusView({
                model: TaxiHail.auth.account
            }).render().el);

        },

        signupconfirmation: function () {
            var view = renderView(TaxiHail.LoginView);
            view.showConfirmationMessage();
        },
        
        book: function () {

            var model = new TaxiHail.Order();
            //default lat and long are defined in the deault.aspx
            TaxiHail.geocoder.initialize(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude);

            TaxiHail.geolocation.getCurrentPosition()
                // By default, set pickup address to current user location
                .done(TaxiHail.postpone(function(address) {
                    model.set('pickupAddress', address);
                }));
            
            mapView.setModel(model, true);
            renderView(TaxiHail.BookView, model);
           
        },

        later: function() {
            var currentOrder = TaxiHail.orderService.getCurrentOrder();
            if (currentOrder && currentOrder.isNew()) {
                renderView(TaxiHail.BookLaterView, currentOrder);
            } else {
                this.navigate('', { trigger: true });
            }
        },
        
        confirmationbook: function () {
            var currentOrder = TaxiHail.orderService.getCurrentOrder();
            if (currentOrder) {
                TaxiHail.auth.account.fetch({
                    success: function(model) {
                        currentOrder.set('settings', model.get('settings'));
                        mapView.setModel(currentOrder);
                        renderView(TaxiHail.BookingConfirmationView, currentOrder);
                    },
                    error: _.bind(function(model) {
                        this.navigate('login/confirmationbook', {trigger: true});
                    }, this)
                });
                
            } else {
                this.navigate('', { trigger: true });
            }
                   
        },
        
        status: function (id) {
            
            var order = new TaxiHail.Order({
                orderId: id
            });

            mapView.goToPickup();

            order.fetch();
            order.getStatus().fetch();

            mapView.setModel(order);
            renderView(TaxiHail.BookingStatusView, order);
       
        },

        
        login: function (url) {
            renderView(TaxiHail.LoginView, new Backbone.Model({
                url : url
            }));
        },
        signup: function () {
            var model = new TaxiHail.NewAccount();
            model.on('sync', function(){
                this.navigate('signupconfirmation', { trigger: true });

            }, this);

            renderView(TaxiHail.SignupView, model);
        },
        
        useraccount: function () {
                TaxiHail.auth.account.fetch({
                    success: function (model) {
                        
                        var account = new TaxiHail.UserAccount(model);
                        renderView(TaxiHail.UserAccountView, account);
                    },
                    error: _.bind(function (model) {
                        this.navigate('login/useraccount', { trigger: true });
                    }, this)
                });
        },
        
        resetpassword : function () {
            renderView(TaxiHail.ResetPasswordView);
        }

    });

}());