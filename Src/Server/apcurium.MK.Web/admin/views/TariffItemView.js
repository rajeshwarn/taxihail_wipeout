(function(){
    
    TaxiHail.TariffItemView = TaxiHail.TemplatedView.extend({

        tagName: 'tr',

        events: {
            'click [data-action=delete]': 'ondelete'
        },

        initialize: function() {
            this.model.on('destroy', this.remove, this);
        },

        render: function() {

            var days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
                selectedDays = [];
            _.each(days, function(dayName, index){
                var day = 1 << index;
                if( day === (this.model.get('daysOfTheWeek') & day) ) {
                    selectedDays.push(dayName);
                }
            }, this);

            var data = this.model.toJSON();
            data.daysOfTheWeek = selectedDays.join(' - ');
            data.recurring = +this.model.get('type') === TaxiHail.Tariff.type.recurring;
            data.isDefault = +this.model.get('type') === TaxiHail.Tariff.type['default'];
            data.isVehicleDefault = +this.model.get('type') === TaxiHail.Tariff.type.vehicleDefault;

            this.$el.html(this.renderTemplate(data));

            return this;
        },

        ondelete: function(e) {
            e.preventDefault();

            TaxiHail.confirm({
                title: this.localize('modal.deleteTariff.title'),
                message: this.localize('modal.deleteTariff.message')
            }).on('ok', function(){
                this.model.destroy();
            }, this);
        }

    });

}());