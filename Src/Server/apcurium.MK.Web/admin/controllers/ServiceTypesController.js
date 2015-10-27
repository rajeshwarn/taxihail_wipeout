(function(TaxiHail){

    var Model = TaxiHail.ServiceType;

    var Collection = TaxiHail.ServiceTypeCollection;

    var Controller = TaxiHail.ServiceTypeController = TaxiHail.Controller.extend({
        initialize: function() {
            this.serviceTypes = new Collection();
            
            $.when(this.serviceTypes.fetch()).then(this.ready);
        },

        index: function () {
            return new TaxiHail.ManageServiceTypesView({
                collection: this.serviceTypes
            });
        },

        edit: function(serviceType) {
            var model = this.serviceTypes.find(function (m) { return m.get('serviceType') == serviceType; });

            var view = new TaxiHail.UpdateServiceTypeView({
                model: model,
                collection: this.serviceTypes
            })
            .on('cancel', function() {
                TaxiHail.app.navigate('serviceTypes', { trigger: true });
            }, this);

            return view;
        }
    });

}(TaxiHail));