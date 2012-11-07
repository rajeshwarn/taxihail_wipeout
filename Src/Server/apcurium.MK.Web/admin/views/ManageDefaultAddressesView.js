(function () {

    TaxiHail.ManageDefaultAddressesView = TaxiHail.TemplatedView.extend({

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

            var $add = $('<a href="#addresses/default/add">').addClass('new').text(TaxiHail.localize('favorites.add-new'));

            $ul.first().append($('<li>').append($add));

            return this;
        },

        edit: function (model) {
            if(!model.isNew()) {
                TaxiHail.app.navigate('addresses/default/edit/' + model.id, {trigger:true});
            }
        }

    });

}());