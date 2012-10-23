(function(TaxiHail, Backbone, _, $){
    
    'use strict';

    var Controller = TaxiHail.Controller = function(){
        this.initialize.apply(this, arguments);
    };

    _.extend(Controller.prototype, {
        initialize: function(){}
    });

    // Static methods
    var currentController;
    Controller.extend = Backbone.Router.extend;
    Controller.action = function(Ctor, action) {
        if(!(currentController instanceof Ctor)) {
            currentController = new Ctor();
            currentController.initialize();
        }
        var view = currentController[action].apply(currentController, Array.prototype.slice(arguments, 2));
        $('#main').html(view.render().el);
    };

}(TaxiHail, Backbone, _, jQuery));