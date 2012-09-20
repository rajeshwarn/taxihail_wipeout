(function () {

    var renderView = function(ctor, model) {
        var view = new ctor({
            model: model
        }).render();

        $('#main').html(view.el);

        return view;

    }, account = new TaxiHail.UserAccount();

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            "": "home",   // #
            "book": "book",   // #book,
            "confirmationbook": "confirmationbook",
            "login": "login", // #login
            "signup": "signup", // #signup
            "signupconfirmation": "signupconfirmation" // redirect to home after signup success
        },

        initialize: function () {
            TaxiHail.auth.on('loggedIn', function() {
                this.navigate('book', { trigger: true });
            }, this);
            
            TaxiHail.auth.on('loggedOut', function () {
                // Clear user account and refetch to trigger redirection to login
                account.clear();
                account.fetch();

            });
            
            $('.login-status-zone').html(new TaxiHail.LoginStatusView({
                model: account
            }).el);
        },

        home: function () {
            renderView(TaxiHail.HomeView);
        },

        signupconfirmation: function () {
            var view = renderView(TaxiHail.HomeView);
            view.showConfirmationMessage();
        },
        
        book: function () {
            account.fetch({
                success: function (model) {
                    var view = renderView(TaxiHail.BookView, new TaxiHail.Order({
                        settings: model.get('settings')
                    })).renderMap();
                }
            });
            
        },
        
        confirmationbook: function () {
            var orderToBook = TaxiHail.store.getItem("orderToBook");
            if (orderToBook) {
                renderView(TaxiHail.BookingConfirmationView, new TaxiHail.Order(orderToBook));
            } else {
                this.navigate('', { trigger: true });
            }
                   
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