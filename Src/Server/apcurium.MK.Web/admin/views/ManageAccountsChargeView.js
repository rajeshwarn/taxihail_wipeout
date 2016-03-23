(function () {

    TaxiHail.ManageAccountsChargeView = TaxiHail.TemplatedView.extend({

        initialize: function () {
            this.collection.on('selected', this.edit, this);
        },

        events: {
            'click [data-action=importdata]': 'importdata'
        },

        importdata: function (e) {
            //e.preventDefault();
            
            // Call the ibschargeaccounts end point
            // Refresh the page on success
        },

        render: function () {

            this.$el.html(this.renderTemplate());

            var $ul = this.$('ul');
            var items = this.collection.reduce(function (memo, model) {
                memo.push(new TaxiHail.AccountChargeItemView({
                    model: model
                }).render().el);
                return memo;
            }, []);

            $ul.first().append(items);

            var $add = $('<a>')
                .attr('href', '#accounts/add')
                .addClass('new')
                .text(this.localize('accountsCharge.add-new'));

            $ul.first().append($('<li>').append($add));

            return this;
        },

        edit: function (model) {
            if(!model.isNew()) {
                TaxiHail.app.navigate('accounts/edit/' + model.get('accountNumber'), { trigger: true });
            }
        }

    });

}());