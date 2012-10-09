(function() {
    TaxiHail.SignupView = TaxiHail.TemplatedView.extend({
    
        tagName: 'form',
        className: 'signup-view form-horizontal',

       events: {
            "submit": "onsubmit",
            "change :text": "onPropertyChanged",
            "change :password": "onPropertyChanged",
            "click [data-action=dosignup]" : "onsubmit"
        },
        
        initialize:function () {
            _.bindAll(this, "onerror");
            $.validator.addMethod(
        "regex",
        function (value, element, regexp) {
            var re = new RegExp(regexp);
            return this.optional(element) || re.test(value);
        }
);

        },

        render: function () {
            this.$el.html(this.renderTemplate());

            this.$el.validate({
                rules: {
                    email: {
                        required:true,
                        email:true
                    },
                    name: "required",
                    phone: {
                        regex: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/,
                        required:true
                    },
                    password: {
                        required: true
                    },
                    confirmPassword: {
                        required: true,
                        equalTo: "#signup-password"
                    }
                },
                messages: {
                    email: {
                        required: TaxiHail.localize('error.EmailRequired'),
                        email: TaxiHail.localize('error.NotEmailFormat')
                    },
                    name: {
                        required: TaxiHail.localize('error.NameRequired')
                    },
                    phone: {
                        required: TaxiHail.localize('error.PhoneRequired'),
                        regex: TaxiHail.localize('error.PhoneBadFormat')
                    },
                    password: {
                        required: TaxiHail.localize('Password required')
                    },
                    confirmPassword: {
                        required: TaxiHail.localize('Password required'),
                        equalTo: TaxiHail.localize('Password are not the same')
                    }
                },
                highlight: function (label) {
                    $(label).closest('.control-group').addClass('error');
                    $(label).prevAll('.valid-input').addClass('hidden');
                }, success: function (label) {
                    $(label).closest('.control-group').removeClass('error');
                    $(label).prevAll('.valid-input').removeClass('hidden');
                    $(label).remove();
                }
            });

            return this;
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            this.model.set($input.attr('name'), $input.val(), {silent: true});
        },
        
        onsubmit: function (e) {
            e.preventDefault();
            if (this.$el.valid()) {
                this.$(':submit').button('loading');
                this.$('.errors').empty();
                this.model.save({}, { error: this.onerror });
            }
        },
        
        onerror: function (model, result) {
            this.$(':submit').button('reset');
            //server validation error
            if (result.statusText) {
               var $alert = $('<div class="alert alert-error" />');
               $alert.text(this.localize(result.statusText));
               this.$('.errors').html($alert);
            }
            
        }
    });
})();