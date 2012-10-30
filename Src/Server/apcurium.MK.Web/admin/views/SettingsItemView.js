(function () {

    TaxiHail.SettingsItemView = TaxiHail.TemplatedView.extend({

        tagName: 'li',

        events: {
            'click [data-action=save-settings]': 'saveSettings',
            'change :input': 'onPropertyChanged'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },

        saveSettings: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget),
                attr = $input.attr('name');

                this.model.set('value', $input.val());
            
        }

    });

}());