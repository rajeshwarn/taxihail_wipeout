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
            $.validator.addMethod("regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp);
                    return this.optional(element) || re.test(value);
                }
            );
            $.validator.addMethod("tenOrMoreDigits",
                function (value, element) {
                    var match = value.match(/\d/g);
                    if (match == null) return false;
                    var count = match.length;
                    return count >= 10;
                }
            );
        },

        render: function () {
            var showPayBackField = false;
            var isPayBackFieldRequired = false;
            if (TaxiHail.parameters.isPayBackRegistrationFieldRequired != null) {
                showPayBackField = true;
                isPayBackFieldRequired = TaxiHail.parameters.isPayBackRegistrationFieldRequired == "true";
            }

            var data = {
                showPayBackField: showPayBackField
            }

            this.$el.html(this.renderTemplate(data));

            var itemfb = TaxiHail.localStorage.getItem('fbinfos');
            var itemtw = TaxiHail.localStorage.getItem('twinfos');
            var infos;
            var validationRules =   {
                email: {
                    required:true,
                    email:true
                },
                name: "required",
                phone: {
                    regex: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})([0-9]?[0-9]?[0-9]?[0-9]?[0-9]?)$/,
                    required: true
                },
                password: {
                    required: true
                },
                confirmPassword: {
                    required: true,
                    equalTo: "#signup-password"
                },
                payback: {
                    required: isPayBackFieldRequired
                }
            };

            if (itemfb) {
                infos = JSON.parse(itemfb);
                TaxiHail.localStorage.removeItem('fbinfos');
            } else {
                if (itemtw) {
                    infos = JSON.parse(itemtw);
                    TaxiHail.localStorage.removeItem('twinfos');
                }
            }
            if (infos) {
                 this.model.set('facebookId', infos.id);
                this.$('#email').val(infos.email);
                this.model.set('email', infos.email);
                this.$('#signup-fullname').val(infos.name);
                this.model.set('name', infos.name);
                this.$('#signup-password-div').addClass('hidden');
                this.$('#signup-confirm-password-div').addClass('hidden');

                                
                validationRules = {
                    email: {
                        required: true,
                        email: true
                    },
                    name: "required",
                    phone: {
                        regex: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})([0-9]?[0-9]?[0-9]?[0-9]?[0-9]?)$/,
                        required: true
                    }                    
                };
            }
           
            this.validate({
                rules: validationRules,
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
            ga('send', 'event', 'button', 'click', 'create account web', 0);
            var lang = TaxiHail.getClientLanguage();
            this.model.set('Language', lang);
            this.model.set('ActivationMethod', 'Email');
            this.model.save({}, { error: _.bind(this.onServerError, this) });
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

})();