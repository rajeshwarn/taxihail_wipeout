// This control is used for Pickup and DropOff address in the Book screen
(function(){

    TaxiHail.AddressControlView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-action=select-address]': 'selectAddress',
            'focus [name=address]': 'onfocus', 
            'blur  [name=address]': 'onblur'
        },

        initialize: function() {
            _.bindAll(this, 'onkeypress');
            this.model.on('change', this.render, this);
        },

        render: function() {

            this.$el.html(this.renderTemplate(this.model.toJSON()));

            this.$('[name=address]').on('keypress', _.debounce(this.onkeypress, 500));


            this._selector = new TaxiHail.AddressSelectionView({
                model: this.model
            }).on('selected', function(model, collection) {
                this.model.set(model.toJSON());
            }, this);
            this._selector.render().hide()
            this.$(".address-selector-container").html(this._selector.el);

            return this;
        },

        selectAddress: function(e) {
            e.preventDefault();

            this.$('[name=address]').focus();

        },

        onfocus: function(e) {
            this._selector && this._selector.show();
        },

        onblur: function(e) {
            //this._selector && this._selector.hide();
        },

        onkeypress: function(e) {
            this._selector && this._selector.search($(e.currentTarget).val());
        }

    });

}());