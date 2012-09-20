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
            "login": "login" // #login
        },

        initialize: function () {
            TaxiHail.auth.on('loggedIn', function() {
                this.navigate('', { trigger: true });
            }, this);
        },

        home: function () {
            renderView(TaxiHail.HomeView);
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
        }

    });

}());