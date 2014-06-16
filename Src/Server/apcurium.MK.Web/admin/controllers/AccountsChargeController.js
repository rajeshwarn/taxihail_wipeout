(function(TaxiHail){

    var Model = TaxiHail.AccountCharge;

    var Collection = TaxiHail.AccountChargeCollection;

    var Controller = TaxiHail.AccountChargeController = TaxiHail.Controller.extend({
        initialize: function() {

            this.accounts = new Collection();
        
            $.when(this.accounts.fetch()).then(this.ready);

        },

        index: function() {
            return new TaxiHail.ManageAccountsChargeView({
                collection: this.accounts
            });
        },

        add: function() {
            var model = new Model({
                isNew: true
            });

            return new TaxiHail.AddAccountChargeView({
                model: model,
                collection: this.accounts
            }).on('cancel', function() {
                TaxiHail.app.navigate('accounts', { trigger: true });
            }, this);
        },

        edit: function(number) {

            var model = this.accounts.find(function(m) { return m.get('number') == number; });
            model.set('isNew', false);
            
            return new TaxiHail.AddAccountChargeView({
                model: model,
                collection: this.accounts
            })
            .on('cancel', function() {
                TaxiHail.app.navigate('accounts', { trigger: true });
            }, this);

        }
    });

}(TaxiHail));