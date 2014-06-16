(function () {
    var currentView,
        renderView = function(view, model) {
            // Call remove on current view
            // in case it was overriden with custom logic
            if(currentView && _.isFunction(currentView.remove)) {
                currentView.remove();
            }

            if(_.isFunction(view)) {
                currentView = new view({
                    model: model
                }).render();
            } else {
                currentView = view;
                view.model = model || view.model;
                view.render();
            }

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
            "gettheapp": "gettheapp", // #gettheapp
            "signup/:url": "signup",
            "signupconfirmation": "signupconfirmation",
            "signupconfirmation/:url": "signupconfirmation",
            "status/:id": "status",
            "useraccount": "useraccount",
            "useraccount/:tab": "useraccount",
            "resetpassword": "resetpassword",
            "bookaccountcharge": "bookaccountcharge",
        },

        initialize: function (options) {
            options = options || {};

            var expire = new Date();
            expire.setTime(expire.getTime() + 3600000 * 24 * 365);
            document.cookie = "ss-opt=perm" + ";expires=" + expire.toGMTString();

            //default lat and long are defined in the default.aspx
            TaxiHail.geocoder.initialize(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude);
            
            TaxiHail.auth.initialize(options.account);
            if( TaxiHail.auth.isLoggedIn() ) {
                // Check if an order exists
                // If order is not saved, go to confirmation
                // If order is saved and status is active, go to status
                var order = TaxiHail.orderService.getCurrentOrder();
                if(order) {
                    if(order.isNew()){
                        //this.navigate('confirmationbook', { trigger: true });
                        this.navigate('', { trigger: true });
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
        
        signupconfirmation: function (url) {
            var fbId = TaxiHail.localStorage.getItem('fbId');
            var twId = TaxiHail.localStorage.getItem('twId');
            var accountActivationDisabled = TaxiHail.parameters.accountActivationDisabled;
            
            var $email = $('[name=email]').val(),
                $password = $('[name=password]').val();

            if (fbId) {
                if (url) {
                    TaxiHail.auth.fblogin(url);
                } else {
                    TaxiHail.auth.fblogin();
                }
            } else if (twId) {
                if (url) {
                    TaxiHail.auth.twlogin(url);
                } else {
                    TaxiHail.auth.twlogin();
                }
            } else if (accountActivationDisabled) {
                if (url) {
                    TaxiHail.auth.login($email, $password, url);
                } else {
                    TaxiHail.auth.login($email, $password, '');
                }
            } else {
                var view = renderView(new TaxiHail.LoginView({
                    returnUrl: url
                }));
                view.showConfirmationMessage();
            }
        },
        
        book: function () {

            
            var model = new TaxiHail.Order();

            TaxiHail.geolocation.initialize();
            

            TaxiHail.geolocation.getCurrentAddress()
            //    // By default, set pickup address to current user location
                .done(TaxiHail.postpone(function (address) {
                
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

        bookaccountcharge:function()
        {
            var currentOrder = TaxiHail.orderService.getCurrentOrder();
            renderView(TaxiHail.BookAccountChargeView, currentOrder);
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
            renderView(new TaxiHail.LoginView({
                returnUrl: url
            }));
        },
        signup: function (url) {
            var model = new TaxiHail.NewAccount();
            model.on('sync', function () {
                if (url) {
                    this.navigate('signupconfirmation/' + url, { trigger: true });
                } else {
                    this.navigate('signupconfirmation', { trigger: true });
                }

            }, this);

            renderView(TaxiHail.SignupView, model);
        },
        
        gettheapp: function () {
           
            renderView(new TaxiHail.GetTheAppView());
        },
        
        useraccount: function (tabName) {
            tabName = tabName || 'profile';
            TaxiHail.auth.account.fetch({
                success: function (model) {

                    if(!(currentView instanceof TaxiHail.UserAccountView)) {
                        var account = new TaxiHail.UserAccount(model);
                        renderView(TaxiHail.UserAccountView, account);
                    }
                    currentView.selectTab(tabName);

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