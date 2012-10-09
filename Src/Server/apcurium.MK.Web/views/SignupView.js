(function() {
    var View = TaxiHail.SignupView = TaxiHail.TemplatedView.extend({
    
        tagName: 'form',
        className: 'signup-view form-horizontal',

       events: {
            "change :text": "onPropertyChanged",
            "change :password": "onPropertyChanged"
        },

        initialize:function () {
            _.bindAll(this, "onsubmit");
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

            this.validate({
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
                submitHandler: this.onsubmit
            });

            return this;
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            this.model.set($input.attr('name'), $input.val(), {silent: true});
        },
        
        onsubmit: function (form) {
            this.model.save({}, { error: this.onServerError });
        }
        
        
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

})();