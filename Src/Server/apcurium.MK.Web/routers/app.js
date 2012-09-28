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
            "confirmationbook": "confirmationbook",
            "login": "login", // #login
            "signup": "signup", // #signup
            "signupconfirmation": "signupconfirmation", // redirect to home after signup success
            "status/:id" : "status"
        },

        initialize: function () {
            TaxiHail.auth.on('change', function(isloggedIn) {
                if(isloggedIn){
                    this.navigate('confirmationbook', { trigger: true });
                }else {
                    this.navigate('', { trigger: true });
                }
            }, this);

            TaxiHail.auth.initialize();

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
            
            mapView.setModel(model, true);

            renderView(TaxiHail.BookView, model);
           
        },
        
        confirmationbook: function () {
            var orderToBook = TaxiHail.store.getItem("orderToBook");
            if (orderToBook) {
                TaxiHail.auth.account.fetch({
                    success: function(model) {
                        orderToBook.settings = model.get('settings');
                        var orderModel = new TaxiHail.Order(orderToBook);
                        mapView.setModel(orderModel);
                        renderView(TaxiHail.BookingConfirmationView, orderModel);
                    },
                    error: _.bind(function(model) {
                        this.navigate('login', {trigger: true});
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

        
        login: function () {
            renderView(TaxiHail.LoginView);
        },
        signup: function () {
            var model = new TaxiHail.NewAccount();
            model.on('sync', function(){
                this.navigate('signupconfirmation', { trigger: true });

            }, this);

            renderView(TaxiHail.SignupView, model);
        }

    });

}());