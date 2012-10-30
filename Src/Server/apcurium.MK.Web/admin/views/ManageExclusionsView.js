(function(){

    var View  = TaxiHail.ManageExclusionsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',

        render: function() {

            this.$el.html(this.renderTemplate(this.model.toJSON()));

            this.validate({
                submitHandler: this.save
            });

            return this;
        },

        save: function(form) {

            var data = this.serializeForm(form);

        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());