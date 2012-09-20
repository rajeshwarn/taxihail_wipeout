(function () {

    TaxiHail.SettingsEditView = TaxiHail.TemplatedView.extend({
        
        tagName:"div",
        events: {
            'change :text': 'onPropertyChanged',
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            this.$("input").attr("disabled", true);
            return this;
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            this.model.set($input.attr('name'), $input.val(), {silent:true});
        }
        
    });
}());