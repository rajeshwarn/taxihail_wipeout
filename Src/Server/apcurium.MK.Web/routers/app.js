(function () {

    var renderView = function(ctor) {
        $('#main').html(new ctor().render().el);
    };

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            "": "book" ,   // #
            "login": "login" // #login
        },

        initialize: function () {
            TaxiHail.auth.on('loggedIn', function() {
                this.navigate('', { trigger: true });
            }, this);
        },

        book: function () {
            renderView(TaxiHail.BookView);
        },
        
        login: function () {
            renderView(TaxiHail.LoginView);
        }
    });
    
    

}());