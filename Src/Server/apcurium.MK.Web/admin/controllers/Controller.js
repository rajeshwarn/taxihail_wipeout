(function(TaxiHail, Backbone, _, $){
    
    'use strict';

    var Controller = TaxiHail.Controller = function(){
        this._dfd = new $.Deferred();
        this._dfd.promise(this);
        this.ready = this._dfd.resolve;
        this.initialize.apply(this, arguments);
    };

    _.extend(Controller.prototype, {
        initialize: function(){}
    });

    // Static methods
    var currentController, currentView;
    Controller.extend = Backbone.Router.extend;
    Controller.action = function(Ctor, action) {
        var actionParams = Array.prototype.slice.call(arguments, 2);
        if(!(currentController instanceof Ctor)) {
            currentController = new Ctor();
            currentController.initialize();
        }
        currentController.then(_.bind(function() {
            var previousView = currentView;
            previousView && previousView.remove();
            currentView = currentController[action].apply(currentController, actionParams);
            $('#main').html(currentView.render().el);
        }, this));
    };

}(TaxiHail, Backbone, _, jQuery));