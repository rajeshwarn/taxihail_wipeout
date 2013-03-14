(function(){
    
    TaxiHail.RuleItemView = TaxiHail.TemplatedView.extend({

        tagName: 'tr',

        events: {
            'click [data-action=delete]': 'ondelete',
            'click [data-action=enable]': 'onEnableDisable'
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
            if (data.appliesToCurrentBooking == true && data.appliesToFutureBooking == true) {
                data.applies = "Both";
            }
            else if (data.appliesToCurrentBooking == true) {
                data.applies = "Current";
            }
            else if (data.appliesToFutureBooking == true) {
                data.applies = "Future";
            } else {
                data.applies = "None";
            }
            
            

            data.daysOfTheWeek = selectedDays.join(' - ');
            data.recurring = +this.model.get('type') === TaxiHail.Tariff.type.recurring;
            data.isDefault = +this.model.get('type') === TaxiHail.Tariff.type['default'];

            this.$el.html(this.renderTemplate(data));
            //data.isActive == true ? 
            data.isActive == true ? this.$('[data-action=enable]').text(this.localize("Disable")) : this.$('[data-action=enable]').text(this.localize("Enable"));
             
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

            TaxiHail.confirm({
                title: this.localize('modal.deleteTariff.title'),
                message: this.localize('modal.deleteTariff.message')
            }).on('ok', function () {
                this.model.destroy();
            }, this);
        },
        
        onEnableDisable: function(e) {
            e.preventDefault();
            var data = this.model.toJSON();
            data.isActive = !data.isActive;
            this.model.save(data, {
                success: _.bind(function (model) {
                    this.render();
                }, this),
                error: function (model, xhr, options) {
                    this.$(':submit').button('reset');

                    var alert = new TaxiHail.AlertView({
                        message: TaxiHail.localize(xhr.statusText),
                        type: 'error'
                    });
                }
            });



    }

    });

}());