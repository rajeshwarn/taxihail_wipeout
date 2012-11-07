(function () {

    var View = TaxiHail.AddFavoriteView = TaxiHail.TemplatedView.extend({

        className: 'add-favorite-view',

        events: {
            "change :input": "onPropertyChanged",
            'focus [name=fullAddress]': 'onfocus',
            'click [data-action=destroy]': 'destroyAddress',
            'click [data-action=cancel]': 'cancel'
        },

        initialize: function () {
            _.bindAll(this, 'save', 'onkeyup', 'ondocumentclick');
            this.model.on('sync', this.refreshParent, this);
            _.defer(_.bind(function () { $(document).on('click', this.ondocumentclick); }, this));
        },

        refreshParent: function () {
            this.collection.trigger('sync');
        },

        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);

            this.validate({
                rules: {
                    friendlyName: "required",
                    fullAddress: "required"
                },
                messages: {
                    friendlyName: {
                        required: TaxiHail.localize('error.friendlyNameRequired')
                    },
                    fullAddress: {
                        required: TaxiHail.localize('error.fullAddressRequired')
                    }
                },
                submitHandler: this.save
            });

            //search address for full address
            this.$('[name=fullAddress]').on('keyup', _.debounce(this.onkeyup, 500));
            this._selector = new TaxiHail.AddressSelectionView({
                model: this.model,
                showFavorites: false,
                showPlaces: this.options.showPlaces
            }).on('selected', function (model, collection) {
                this.model.set(model.toJSON());
                this.close();
            }, this);


            this._selector.render().hide();
            this.$(".address-selector-container").html(this._selector.el);

            return this;
        },

        save: function (form) {
            if (this.model.has('fullAddress')) {
                if (!this.model.get('isHistoric')) {
                    this.model.save();
                } else {
                    this.collection.create(_.pick(this.model.toJSON(), 'friendlyName', 'fullAddress', 'apartment', 'ringCode', 'buildingName'), { wait: true });
                }
            }
        },

        destroyAddress: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('Remove Favorite Address'),
                message: this.localize('modal.removeFavorite.message')
            }).on('ok', function () {
                this.model.destroy();
            }, this);
        },

        remove: function () {
            if (this._selector) this._selector.remove();
            $(document).off('click', this.ondocumentclick);
            this.$el.remove();
            return this;
        },

        open: function (e) {
            e && e.preventDefault();

            this.trigger('open', this);
            if (this._selector) {
                this._selector.show();
            }

        },

        close: function () {
            var $input = this.$('[name=fullAddress]');

            this._selector && this._selector.hide();
            // Set address in textbox back to the value of the model
            $input.val(this.model.get('fullAddress'));
            // Force validation of the Address field
            this.$('form').data().validator.element($input);
        },

        onfocus: function (e) {
            this.open();
        },

        onkeyup: function (e) {
            if (!jQuery.hotkeys.specialKeys[e.which]) {
                this._selector && this._selector.search($(e.currentTarget).val());
            }
        },

        cancel: function (e) {
            e.preventDefault();
            this.model.set(this.model.previousAttributes);
            this.trigger('cancel', this);
        },

        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            if ($input.attr('name') === 'fullAddress') {
                return;
            }

            this.model.set($input.attr("name"), $input.val());
        },

        ondocumentclick: function (e) {
            if (!this.$('.address-picker').find(e.target).length) {
                this.close();
            }
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());