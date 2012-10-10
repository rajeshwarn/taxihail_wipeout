(function () {
    
    var View = TaxiHail.ProfileView = TaxiHail.TemplatedView.extend({
        events: {
            'change :input': 'onPropertyChanged'
        },

        initialize: function () {

            _.bindAll(this, 'savechanges');

            this.referenceData = new TaxiHail.ReferenceData();
            this.referenceData.fetch();
            this.referenceData.on('change', this.render, this);

            $.validator.addMethod(
                "regex",
                function(value, element, regexp) {
                    var re = new RegExp(regexp);
                    return this.optional(element) || re.test(value);
                }
                
            );

        },

        render: function () {
            var data = this.model.toJSON();

            _.extend(data, {
                vehiclesList: this.referenceData.attributes.vehiclesList,
                paymentsList: this.referenceData.attributes.paymentsList
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
        
        savechanges : function (form) {
            this.model.updateSettings()
                .done(_.bind(function () {
                    this.$("#notif-bar").html(TaxiHail.localize('Settings Changed'));
                    this.$(':submit').button('reset');
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
