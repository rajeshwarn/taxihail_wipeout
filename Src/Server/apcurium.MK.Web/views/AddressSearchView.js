(function(){

    TaxiHail.AddressSearchView = TaxiHail.TemplatedView.extend({


        initialize: function(){
            _.bindAll(this, 'onkeypress', 'renderResults');
        },

        render: function() {

            this.$el.html(this.renderTemplate());

            this.$('input.search-query').on('keypress', _.debounce(this.onkeypress, 500));

            new TaxiHail.AddressListView({
                collection : this.collection,
                el: this.$('ul.search-results')
            });

            return this;
        },

        onkeypress: function(e) {
            var address = $(e.currentTarget).val()
                
                TaxiHail.geocoder.geocode(address)
                    .done(this.renderResults);
        },

        renderResults: function(result) {

            this.collection.reset(result.addresses);           

        }

    });

}());