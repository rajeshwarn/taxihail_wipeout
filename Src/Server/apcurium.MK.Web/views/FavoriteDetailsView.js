(function () {

    TaxiHail.FavoriteDetailsView = TaxiHail.TemplatedView.extend({
        events: {
        },
        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);
            
            


            return this;
        },

    });

}());