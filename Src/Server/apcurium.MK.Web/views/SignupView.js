(function() {
    TaxiHail.SignupView = TaxiHail.TemplatedView.extend({
        
        tagName: "form",
        
        events: {
            "submit": "onsubmit",
            "change :text": "onPropertyChanged",
            "change :password": "onPropertyChanged"
        },
        
        initialize:function () {
            this.$el.addClass('form-horizontal');                       
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
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            this.model.set($input.attr('name'), $input.val(), { silent: true });
        },
        
        onsubmit: function (e) {
            e.preventDefault();
            this.$('.errors').empty();
            this.model.save({}, { error: this.onerror });           
        },
        
        onerror: function (model, result) {           
            //server validation error
            if (result.statusText) {
               var $alert = $('<div class="alert alert-error" />');
               $alert.text(this.localize(result.statusText));
               this.$('.errors').html($alert);
            }              
            
        }       
        
    });
})();