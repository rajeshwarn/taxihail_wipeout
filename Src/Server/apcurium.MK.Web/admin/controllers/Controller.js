(function(TaxiHail, Backbone, _, $){
    
    'use strict';

    var Controller = TaxiHail.Controller = function(){
        this.initialize.apply(this, arguments);
        this._dfd = new $.Deferred();
        this._dfd.promise(this);
        this.ready = this._dfd.resolve;
    };

    _.extend(Controller.prototype, {
        initialize: function(){}
    });

    // Static methods
    var currentController;
    Controller.extend = Backbone.Router.extend;
    Controller.action = function(Ctor, action) {
        var actionParams = Array.prototype.slice.call(arguments, 2);
        if(!(currentController instanceof Ctor)) {
            currentController = new Ctor();
            currentController.initialize();
        }
        currentController.then(function() {
            var view = currentController[action].apply(currentController, actionParams);
            $('#main').html(view.render().el);
        });
    };

}(TaxiHail, Backbone, _, jQuery));