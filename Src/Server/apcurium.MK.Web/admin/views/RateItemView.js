(function(){
    
    TaxiHail.RateItemView = TaxiHail.TemplatedView.extend({

        tagName: 'li',

        render: function() {

            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        }

    });

}());