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

        renderResults: function(result) {

            var $list = $('ul.search-results').empty();
            _.each(result.addresses, function(address) {
                $list.append(new TaxiHail.AddressItemView({
                    model: new TaxiHail.Address(address)
                }).render().el);
            }, this);

        }

    });

}());