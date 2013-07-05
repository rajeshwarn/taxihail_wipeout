(function(){

    TaxiHail.ValidatedView = {

        validationDefaults: {
            errorElement: 'span',
            errorClass: 'help-inline',
            highlight: function (element, errorClass) {
                $(element).closest('.control-group')
                    .addClass('error')
                    .removeClass('success');
                $(element).siblings('.' + errorClass ).show();
            },
            unhighlight: function(element, errorClass) {
                $(element).closest('.control-group').removeClass('error');
                $(element).siblings('.' + errorClass ).hide();
            },
            success: function(label) {
                $(label).hide()
                    .closest('.control-group')
                    .removeClass('error')
                    .addClass('success');
            }
        },

        validate: function(options) {
            var submitHandler = options.submitHandler;
            options = _.extend({}, options, this.validationDefaults);

            options.submitHandler = _.bind(this.defaultSubmitHandler, this, submitHandler);

            var $form = this.$el.find('form').andSelf().filter('form');

            $form.validate(options);
        },

        defaultSubmitHandler: function(callback, form) {
            $(form).find(':submit').button('loading');
            $(form).find('.errors').empty();

            if(callback) callback.call(this, form);
        },

        onServerError: function (model, result) {
            this.$(':submit').button('reset');
            //server validation error
            if (result.statusText) {
               var alert = new TaxiHail.AlertView({
                   message: this.localize(result.statusText),
                   type: 'error'
               });
               alert.on('ok', alert.remove, alert);
               this.$('.errors').html(alert.render().el);
            }
        },

        serializeForm: function(form) {
            var o = {};

           var a = $(form).serializeArray();
           $.each(a, function() {
               if (o[this.name]) {
                   if (!o[this.name].push) {
                       o[this.name] = [o[this.name]];
                   }
                   o[this.name].push(this.value || '');
               } else {
                   o[this.name] = this.value || '';
               }
           });
           return o;
        }

    };

    (function ($) {
        $.fn.serializeObjectChris = function () {

            var thisThis = this;

            var regex = {
                getRoot: /^[a-z|A-Z]+\./,
                getMember: /\.[a-z|A-Z]+/,
            };

            var json = {};
            
            this.build = function (base, key, value, remaining) {
                
                var member = remaining.match(regex.getMember);
                if (member) {
                    var nextKey = member[0].replace(".", "");
                    var newRemaining = remaining.substring(member.index + member[0].length);
                    
                    if (!base[key]) {
                        base[key] = {};
                    }

                    base[key] = thisThis.build(base[key], nextKey, value, newRemaining);
                    
                } else {
                    base[key] = value;
                }


                return base;
            };


            $.each($(this).serializeArray(), function () {
                var root = this.name.match(regex.getRoot);
                if (!root) {
                    json[this.name] = this.value;
                    return;
                }
                
                root = root[0].replace(".", "");

                if (!json[root]) {
                    json[root] = {};
                }

                thisThis.build(json, root, this.value, this.name);


            });

            return json;
        };
    })(jQuery);

    (function ($) {
        $.fn.serializeObject = function () {

            var self = this,
                json = {},
                push_counters = {},
                patterns = {
                    "validate": /^[a-zA-Z][a-zA-Z0-9_]*(?:\[(?:\d*|[a-zA-Z0-9_]+)\])*$/,
                    "key": /[a-zA-Z0-9_]+|(?=\[\])/g,
                    "push": /^$/,
                    "fixed": /^\d+$/,
                    "named": /^[a-zA-Z0-9_]+$/
                };


            this.build = function (base, key, value) {
                base[key] = value;
                return base;
            };

            this.push_counter = function (key) {
                if (push_counters[key] === undefined) {
                    push_counters[key] = 0;
                }
                return push_counters[key]++;
            };

            $.each($(this).serializeArray(), function () {

                // skip invalid keys
                if (!patterns.validate.test(this.name)) {
                    return;
                }

                var k,
                    keys = this.name.match(patterns.key),
                    merge = this.value,
                    reverse_key = this.name;

                while ((k = keys.pop()) !== undefined) {

                    // adjust reverse_key
                    reverse_key = reverse_key.replace(new RegExp("\\[" + k + "\\]$"), '');

                    // push
                    if (k.match(patterns.push)) {
                        merge = self.build([], self.push_counter(reverse_key), merge);
                    }

                        // fixed
                    else if (k.match(patterns.fixed)) {
                        merge = self.build([], k, merge);
                    }

                        // named
                    else if (k.match(patterns.named)) {
                        merge = self.build({}, k, merge);
                    }
                }

                json = $.extend(true, json, merge);
            });

            return json;
        };
    })(jQuery);






}());