(function () {

    TaxiHail.ManageVehicleTypesView = TaxiHail.TemplatedView.extend({

        initialize: function () {
            this.collection.on('selected', this.edit, this);
        },

        render: function () {

            this.$el.html(this.renderTemplate());

            var $ul = this.$('ul');
            var items = this.collection.reduce(function (memo, model) {
                memo.push(new TaxiHail.VehicleTypeItemView({
                    model: model
                }).render().el);
                return memo;
            }, []);

            $ul.first().append(items);

            var $add = $('<a>')
                .attr('href', '#vehicleTypes/add')
                .addClass('new')
                .text(this.localize('vehicleType.add-new'));

            $ul.first().append($('<li>').append($add));

            return this;
        },

        edit: function (model) {
            if(!model.isNew()) {
                TaxiHail.app.navigate('vehicleTypes/edit/' + model.get('id'), { trigger: true });
            }
        }

    });

}());