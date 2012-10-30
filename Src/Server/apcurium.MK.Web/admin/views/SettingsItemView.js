(function () {
    var canExecute = true;
    TaxiHail.SettingsItemView = TaxiHail.TemplatedView.extend({
        
        tagName: 'div',

        events: {
            'click [data-action=save-settings]': 'saveSettings',
            'change :input': 'onPropertyChanged'
        },
        
        initialize: function () {
            this.model.on('sync', this.changeButtonState, this);
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            return this;
        },

        saveSettings: function (e) {
            e.preventDefault();
            if (canExecute == true) {
                 this.model.trigger('selected', this.model, this.model.collection);
            }
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
                this.model.set('value', $input.val());
            
        },
        changeButtonState: function (e) {
            canExecute = false;
            var attr = this.model.attributes.key;
            $(".btn[name='" + attr + "']").addClass('btn-success').text('Saved');
            window.setTimeout(function () {
                $(".btn[name='" + attr + "']").removeClass('btn-success').text('Save');
                canExecute = true;
            }, 3000);
        }

    });

}());