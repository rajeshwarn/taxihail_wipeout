(function () {
    
    var View = TaxiHail.ProfileView = TaxiHail.TemplatedView.extend({
        events: {
            'change :input': 'onPropertyChanged'
        },

        initialize: function () {

            _.bindAll(this, 'savechanges');

            $.validator.addMethod(
                "regex",
                function(value, element, regexp) {
                    var re = new RegExp(regexp);
                    return this.optional(element) || re.test(value);
                }
                
            );

            this.render();

        },

        render: function () {
            var data = this.model.toJSON();

            _.extend(data, {
                vehiclesList: TaxiHail.vehicleTypes,
                paymentsList: TaxiHail.referenceData.paymentsList
            });

            this.$el.html(this.renderTemplate(data));
            
            this.validate({
                rules: {
                    name: "required",
                    phone: {
                        required : true,
                        regex: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/
                    },
                    passengers: {
                        required: true,
                        number : true
                    }
                },
                messages: {
                    name: {
                        required: TaxiHail.localize('error.NameRequired')
                    },
                    phone: {
                        required: TaxiHail.localize('error.PhoneRequired'),
                        regex: TaxiHail.localize('error.PhoneBadFormat')
                    },
                    passengers: {
                        required: TaxiHail.localize('error.PassengersRequired'),
                        number: TaxiHail.localize('error.NotANumber')
                    }
                },
                submitHandler: this.savechanges
            });

            return this;
        },

        renderConfirmationMessage: function() {
            var view = new TaxiHail.AlertView({
                message: this.localize('Changes were saved'),
                type: 'success'
            });
            view.on('ok', this.render, this);
            this.$el.html(view.render().el);
        },
        
        savechanges : function (form) {
            this.model.updateSettings()
                .done(_.bind(function () {
                    this.renderConfirmationMessage();
                }, this))
                .fail(_.bind(function(){
                    this.$(':submit').button('reset');
                }, this));
        },
        
        onPropertyChanged : function (e) {
            var $input = $(e.currentTarget);
            var settings = this.model.get('settings');

            settings[$input.attr("name")] = $input.val();
            this.$(':submit').removeClass('disabled');
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());
