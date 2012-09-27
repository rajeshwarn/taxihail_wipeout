(function () {

    TaxiHail.SettingsEditView = TaxiHail.TemplatedView.extend({
        
        tagName:"div",
        events: {
            'change :input': 'onPropertyChanged',
        },
        
        initialize: function () {
            this.referenceData = new TaxiHail.ReferenceData();
            this.referenceData.fetch();
            this.referenceData.on('change', this.render, this);
            
        },

        render: function () {
            
            Handlebars.registerHelper('ifCond', function (v1, v2, options) {
                if (v1 == v2) {
                    return options.fn(this);
                } else {
                    return options.inverse(this);
                }
            });

            var data = this.model.toJSON();

            _.extend(data, {
                vehiclesList : this.referenceData.attributes.vehiclesList,
                paymentsList : this.referenceData.attributes.paymentsList
            });

            this.$el.html(this.renderTemplate(data));
            return this;
        },

        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            this.model.set($input.attr('name'), $input.val(), {silent:true});
        }
        
    });
}());