(function() {
    TaxiHail.SignupView = TaxiHail.TemplatedView.extend({
        
        tagName: "form",
        
        events: {
            "submit": "onsubmit",
            "change :text": "onPropertyChanged"
        },
        
        initialize:function () {
            this.$el.addClass('form-horizontal');
            this.model = new TaxiHail.NewAccount();
            _.bindAll(this, "onerror", "onsuccess");
        },

        render: function () {
            this.$el.html(this.renderTemplate());
            return this;
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            this.model.set($input.attr('name'), $input.val(), { silent: true });
        },
        
        onsubmit: function (e) {
            e.preventDefault();
            this.$('.errors').empty();
            this.model.save({}, { error: this.onerror, success: this.onsuccess });
        },
        
        onerror: function (model, result) {
            var $alert = $('<div class="alert alert-error" />');
            //local validation error
            if (result.errors) {
               _.each(result.errors, function (error) {
                    $alert.append($('<div />').text(this.localize(error.errorCode)));
                }, this);
               
            } else {
                $alert.text(this.localize(result.statusText));
            }
             
            this.$('.errors').html($alert);
        },
        onsuccess: function () { }
        
    });
})();