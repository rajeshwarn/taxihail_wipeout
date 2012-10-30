(function () {

    TaxiHail.ManageCompanySettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'div',
        className: 'well',
        
        
        
        initialize : function () {
            this.collection.on('selected', this.saveSettings, this);
        },
        
        render: function () {
            this.$el.html(this.renderTemplate());
            
            this.$el.empty();
            if (this.collection.length) {
                this.collection.each(this.renderItem, this);
                //this.$el.append($('<button>').addClass('btn save-settings').text(TaxiHail.localize('settings.save')));
            } else {
                this.$el.append($('<li>').addClass('no-result').text(TaxiHail.localize('settings.no-result')));
            }

            return this;
        },

        renderItem: function (model) {

            var itemView = new TaxiHail.SettingsItemView({
                model: model
            });

            this.$el.append(itemView.render().el);
        },
        
        saveSettings: function (model) {

            model.save();
        }

    });

}());