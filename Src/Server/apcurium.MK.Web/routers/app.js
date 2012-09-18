(function () {

    var renderView = function(ctor, model) {
        $('#main').html(new ctor({
            model: model
        }).render().el);
    };

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            "": "home",   // #
            "book": "book",   // #book
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
            renderView(TaxiHail.BookView, new TaxiHail.Order());
        },
        
        login: function () {
            renderView(TaxiHail.LoginView);
        }
    });
    
    

}());