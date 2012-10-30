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
               var $alert = $('<div class="alert alert-error" />');
               $alert.text(this.localize(result.statusText));
               this.$('.errors').html($alert);
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

}());