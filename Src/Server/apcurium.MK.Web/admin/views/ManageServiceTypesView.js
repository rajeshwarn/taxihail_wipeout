(function () {

    TaxiHail.ManageServiceTypesView = TaxiHail.TemplatedView.extend({

        initialize: function () {
            this.collection.on('selected', this.edit, this);
        },

        render: function () {

            this.$el.html(this.renderTemplate());

            var $ul = this.$('ul');
            var items = this.collection.reduce(function (memo, model) {
                memo.push(new TaxiHail.ServiceTypeItemView({
                    model: model
                }).render().el);
                return memo;
            }, []);

            $ul.first().append(items);

            return this;
        },

        edit: function (model) {
            TaxiHail.app.navigate('serviceTypes/edit/' + model.get('serviceType'), { trigger: true });
        },
    });

}());