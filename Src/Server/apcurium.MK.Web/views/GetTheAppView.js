(function() {
    TaxiHail.GetTheAppView = TaxiHail.TemplatedView.extend({
    
        

        initialize:function () {
        },

        render: function () {
            this.$el.html(this.renderTemplate());
            return this;
        },
       
    });


})();