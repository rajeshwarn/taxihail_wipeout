// This control is used for Pickup and DropOff address in the Book screen
(function(){

    TaxiHail.AddressControlView = TaxiHail.TemplatedView.extend({

        className: 'address-picker',

        events: {
            'click [data-action=clear]': 'clear',
            'click [data-action=locate]': 'locate',
            'click [data-action=refine]': 'refine',
            'click [data-action=toggletarget]': 'toggletarget',
            'focus [name=address]': 'onfocus'
        },

        initialize: function(attrs, options) {
            _.bindAll(this, 'onkeyup', 'ondocumentclick');
            this.model.on('change', this.render, this);

            $(document).on('click', this.ondocumentclick);
        },
        
        toggletarget: function (e) {
            e && e.preventDefault();

            this.trigger('target', this, !$(e.currentTarget).is('.active'));
        },

        render: function() {

            // Keep state of toggle button before re-rendering
            var toggleClass = this.$("[data-action=toggletarget]").attr('class');

            var data = _.extend(this.model.toJSON(), {
                options: _.pick(this.options, 'locate', 'clear', 'pin', 'refine')
            });

            this.$el.html(this.renderTemplate(data));

            var refineBtn = this.$el.find('[data-action=refine]');
            var streetNumber = this.model.get('streetNumber');
            if (!streetNumber) {
                refineBtn.addClass('disabled');
                refineBtn.attr('disabled', 'disabled');
            } else {
                refineBtn.removeClass('disabled');
                refineBtn.removeAttr('disabled');
            }

            this.$("[data-action=toggletarget]").addClass(toggleClass);

            this.$('[name=address]').on('keyup', _.debounce(this.onkeyup, 1500));

            this._selector = new TaxiHail.AddressSelectionView({
                model: this.model
            }).on('selected', function(model, collection) {
                this.model.clear({ silent: true });
                this.model.set(model.toJSON());
                this.close();
            }, this);

            this._selector.render().hide();
            this.$(".address-selector-container").html(this._selector.el);

            return this;
        },

        remove: function() {
            if(this._selector) this._selector.remove();
            $(document).off('click', this.ondocumentclick);
            this.$el.remove();
            return this;
        },

        open: function(e) {
            e && e.preventDefault();

            // Deactivate the "target" button
            this.toggleOff();
            this.trigger('target', this, false);

            this.trigger('open', this);
            if(this._selector)
            {
                this._selector.show();
            }

        },

        close: function() {
            this._selector && this._selector.hide();
            // Set address in textbox back to the value of the model
            this.$('[name=address]').val(this.model.get('fullAddress'));
        },

        toggleOff: function(){
            this.$('[data-action=toggletarget]').removeClass('active');
        },

        locate: function (e) {

            var opts = {
                lines: 11, // The number of lines to draw
                length: 3, // The length of each line
                width: 3, // The line thickness
                radius: 6, // The radius of the inner circle
                corners: 1, // Corner roundness (0..1)
                rotate: 0, // The rotation offset
                speed: 1, // Rounds per second
                trail: 60, // Afterglow percentage
                shadow: false, // Whether to render a shadow
                hwaccel: false, // Whether to use hardware acceleration
                className: 'spinner', // The CSS class to assign to the spinner
                zIndex: 2e9 // The z-index (defaults to 2000000000)
            };
            
            var $spinContainer = $('<div class="icon-locating" />');
            var spinner = new Spinner(opts).spin();
            $spinContainer.append(spinner.el);
            
            var $button = $(e.currentTarget).children().first();
            var $arrow = $button.replaceWith($spinContainer);

            // Deactivate the "target" button
            this.toggleOff();
            this.trigger('target', this, false);

            
            TaxiHail.geolocation.getCurrentAddress()
                .done(_.bind(function(address){
                    this.model.set(address);
                }, this))
                .always(function (address) {
                    spinner.stop();
                    $spinContainer.replaceWith($arrow);
                });
        },

        clear: function() {
            this.model.clear();
            this.close();
        },

        refine: function() {
            var originalStreetNumber = this.model.get('streetNumber');
            var fullAddress = this.model.get('fullAddress');
            if (!originalStreetNumber || !fullAddress) {
                // don't show the prompt if there's no address
                return;
            }

            var promptTitle = TaxiHail.localize('RefineAddress');
            var newStreetNumber = prompt(promptTitle, originalStreetNumber);
            if (!newStreetNumber) {
                // don't replace if the user cancelled or entered string.empty
                return;
            }

            this.model.set('streetNumber', newStreetNumber);
            this.model.set('fullAddress', fullAddress.replace(originalStreetNumber, newStreetNumber));
        },

        onfocus: function(e) {
            this.open();
        },

        onkeyup: function(e) {
            //if(!jQuery.hotkeys.specialKeys[ e.which ]) {

            if ($(e.currentTarget).val()!="") {
                this._selector && this._selector.search($(e.currentTarget).val());
            }
            //}
        },

        ondocumentclick: function(e) {
            if(!this.$el.find(e.target).length) {
                this.close();
            }
        }

    });

}());