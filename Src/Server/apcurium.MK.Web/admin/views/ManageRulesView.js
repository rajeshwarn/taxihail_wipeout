(function(TaxiHail){

    "use strict";

    TaxiHail.ManageRulesView = TaxiHail.TemplatedView.extend({

        initialize: function() {
            this.collection.on('reset', this.render, this);
            this.collection.reset(this.collection.toJSON());
        },

        render: function() {

            this.$el.html(this.renderTemplate({
                isNetworkEnabled: TaxiHail.parameters.isNetworkEnabled == true
                    || TaxiHail.parameters.isNetworkEnabled == "true"
            }));
            this.collection.each(this.renderItem, this);

            return this;
        },

        renderItem: function(rule) {
            if (rule.get('category') == TaxiHail.Rule.category.warningRule) {
                new TaxiHail.RuleItemView({
                    model: rule
                }).render().$el.appendTo(this.$('tbody[name=warningItem]'));
            } else {
                new TaxiHail.RuleItemView({
                    model: rule
                }).render().$el.appendTo(this.$('tbody[name=disableItem]'));
            }
           
            
        }
    });


}(TaxiHail));