(function(TaxiHail){

    "use strict";

    TaxiHail.ManageRulesView = TaxiHail.TemplatedView.extend({

        initialize: function() {
            this.collection.on('reset', this.render, this);
        },

        render: function() {

            this.$el.html(this.renderTemplate());
            //this.collection = this.collection.where({ category: TaxiHail.Rule.category.warningRule });
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