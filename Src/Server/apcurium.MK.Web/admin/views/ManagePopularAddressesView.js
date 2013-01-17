(function () {

    TaxiHail.ManagePopularAddressesView = TaxiHail.TemplatedView.extend({

        initialize: function () {
            this.collection.on('selected', this.edit, this);
        },

        render: function () {

            this.$el.html(this.renderTemplate());
            
            var $ul = this.$('ul');
            var items = this.collection.reduce(function (memo, model) {
                memo.push(new TaxiHail.AddressItemView({
                    model: model
                }).render().el);
                return memo;
            }, []);

            $ul.first().append(items);

            var $add = $('<a>')
                .attr('href', '#addresses/popular/add')
                .addClass('new')
                .text(TaxiHail.localize('popular.add-new'));

            $ul.first().append($('<li>').append($add));

            return this;
        },

        edit: function (model) {
            if(!model.isNew()) {
                TaxiHail.app.navigate('addresses/popular/edit/' + model.id, {trigger:true});
            }
        }

    });

}());