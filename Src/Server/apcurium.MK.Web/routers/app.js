(function () {

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            "": "book"    //  
        },

        initialize: function () {
        },

        book: function () {
            $('#main').html(new TaxiHail.BookView().render().el);
        }
    });

}());