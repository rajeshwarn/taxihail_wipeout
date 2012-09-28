(function() {
    TaxiHail.SignupView = TaxiHail.TemplatedView.extend({
        
        tagName: "form",
        className: 'signup-view form-horizontal',
        
        events: {
            "submit": "onsubmit",
            "change :text": "onPropertyChanged",
            "change :password": "onPropertyChanged",
            "keyup :text": "onKeyPress",
            "keyup :password": "onKeyPress",
            "blur :text": "onKeyPress",
            "blur :password": "onKeyPress"
        },
        
        initialize:function () {
            _.bindAll(this, "onerror");
        },

        render: function () {
            this.$el.html(this.renderTemplate());
            _.extend(Backbone.Validation.messages, {
                required: this.localize('error.Required'),
                pattern: this.localize('error.Pattern'),
                equalTo: this.localize('error.EqualTo')
            });

            Backbone.Validation.bind(this);
            return this;
        },
        
        onKeyPress: function (e) {

            //ignore tab key
            if (e.keyCode != 9) {
                var $input = $(e.currentTarget);
                var attrName = $input.attr('name');
                var attrValue = $input.val();

                this.model.set(attrName, attrValue, { silent: true });
                var errorMessage = this.model.preValidate(attrName, attrValue);

                if (errorMessage) {
                    //hide valid status image
                    $input.next().addClass('hidden');
                    //display error message
                    Backbone.Validation.callbacks.invalid(this, attrName, errorMessage, "name");
                } else {
                    //show valid status image
                    $input.next().removeClass('hidden');
                    Backbone.Validation.callbacks.valid(this, attrName, "name");
                }
            }
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            this.model.set($input.attr('name'), $input.val(), {silent: true});
        },
        
        onsubmit: function (e) {
            this.$(':submit').button('loading');
            e.preventDefault();
            this.$('.errors').empty();
            this.model.save({}, { error: this.onerror });
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