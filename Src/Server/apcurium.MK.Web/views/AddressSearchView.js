(function(){

    TaxiHail.AddressSearchView = TaxiHail.TemplatedView.extend({


        initialize: function(){
            _.bindAll(this, 'onkeypress', 'renderResults');
        },

        render: function() {

            this.$el.html(this.renderTemplate());

            this.$('input.search-query').on('keypress', _.debounce(this.onkeypress, 500));

            return this;
        },

        onkeypress: function(e) {
            var address = $(e.currentTarget).val()
                
                TaxiHail.geocoder.geocode(address)
                    .done(this.renderResults);
        },

        renderResults: function(results) {

            var $list = $('ul.search-results').empty();
            _.each(results, function(result) {
                $list.append(new TaxiHail.AddressItemView({
                    model: TaxiHail.Address.fromGeocodingResult(result)
                }).render().el);
            }, this);

        }

    });

}());