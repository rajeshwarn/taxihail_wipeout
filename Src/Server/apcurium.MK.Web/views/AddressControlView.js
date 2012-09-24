// This control is used for Pickup and DropOff address in the Book screen
(function(){

    TaxiHail.AddressControlView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-action=open]': 'open',
            'click [data-action=toggleselect]': 'toggleselect',
            'focus [name=address]': 'onfocus', 
            'blur  [name=address]': 'onblur'
        },

        initialize: function() {
            _.bindAll(this, 'onkeypress');
            this.$el.addClass('address-picker');
            this.model.on('change', this.render, this);
        },
        
        toggleselect: function (e) {
            e && e.preventDefault();

            this.trigger('toggleselect', this);
        },

        render: function() {

            this.$el.html(this.renderTemplate(this.model.toJSON()));

            this.$('[name=address]').on('keypress', _.debounce(this.onkeypress, 500));


            this._selector = new TaxiHail.AddressSelectionView({
                model: this.model
            }).on('selected', function(model, collection) {
                this.model.set(model.toJSON());
                this.close();
            }, this);

            this._selector.render().hide()
            this.$(".address-selector-container").html(this._selector.el);

            return this;
        },

        remove: function() {
            if(this._selector) this._selector.remove();
            this.$el.remove();
            return this;
        },

        open: function(e) {
            e && e.preventDefault();

            this.trigger('open', this);
            this._selector && this._selector.show();

        },

        close: function() {
            this._selector && this._selector.hide();
            // Set address in textbox back to the value of the model
            this.$('[name=address]').val(this.model.get('fullAddress'));
        },

        onfocus: function(e) {
            this.open();
        },

        onblur: function(e) {
            //this._selector && this._selector.hide();
        },

        onkeypress: function(e) {
            this._selector && this._selector.search($(e.currentTarget).val());
        }

    });

}());