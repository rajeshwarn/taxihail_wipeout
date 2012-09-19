(function () {

    var renderView = function(ctor, model) {
        $('#main').html(new ctor({
            model: model
        }).render().el);
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
                    renderView(TaxiHail.BookView, new TaxiHail.Order({
                        settings: model.get('settings')
                    }));
                }
            });
            
        },
        
        confirmationbook:function () {
            renderView(TaxiHail.BookingConfirmationView, new TaxiHail.Order({
                settings: model.get('settings')
            }));
        },
        
        login: function () {
            renderView(TaxiHail.LoginView);
        }
    });
    
    

}());