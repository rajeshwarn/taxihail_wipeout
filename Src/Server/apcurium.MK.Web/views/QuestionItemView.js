(function() {

    TaxiHail.QuestionItemView = TaxiHail.TemplatedView.extend({
        
        tagName: 'li',
        
        events: { },

        render: function () {
            this.$el.html(this.renderTemplate(this.model));
            return this;
        }       

    });

}());