(function () {
    var View = TaxiHail.SignupView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'signup-view form-horizontal',

        events: {
            "change :text": "onPropertyChanged",
            "change :password": "onPropertyChanged",
            "change #countrycode": "onPropertyChanged"
        },

        country:new Object(),
    
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

            this.model.set("country", this.country, { silent: true });
            this.country.code = TaxiHail.parameters.defaultCountryCode;
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

            _.extend(data, {
                countryCodes: TaxiHail.extendSpacesForCountryDialCode(TaxiHail.countryCodes),
            });

            this.$el.html(this.renderTemplate(data));

            this.$("#countrycode").val(this.country.code).selected = "true";

            var itemfb = TaxiHail.localStorage.getItem('fbinfos');
            var itemtw = TaxiHail.localStorage.getItem('twinfos');
            var infos;
            var validationRules =   {
                email: {
                    required:true,
                    email:true
                },
                name: "required",
                countryCode:
                    {
                        required:true
                    },
                phone: {
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
                    required: isPayBackFieldRequired,
                    regex: /^\d{0,10}$/ // Up to 10 digits
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
                    },
                    password: {
                        required: TaxiHail.localize('Password required')
                    },
                    confirmPassword: {
                        required: TaxiHail.localize('Password required'),
                        equalTo: TaxiHail.localize('Password are not the same')
                    },
                    payback: {
                        required: TaxiHail.localize('error.PayBackRequired'),
                        regex: TaxiHail.localize('error.PayBackBadFormat')
                    }
                },
                submitHandler: this.onsubmit
            });

            return this;
        },
        
        onPropertyChanged: function (e) {

            var dataNodeName = e.currentTarget.nodeName.toLowerCase();
            var elementName = e.currentTarget.name;

            var $input = $(e.currentTarget);

            if (dataNodeName == "input") {
                var dataValue = $input.val();
                this.model.set($input.attr('name'), $input.val(), { silent: true });
            }
            else if (dataNodeName == "select") {
                if (elementName == "countryCode") {
                    this.country.code = $input.find(":selected").val();
                }
            }
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