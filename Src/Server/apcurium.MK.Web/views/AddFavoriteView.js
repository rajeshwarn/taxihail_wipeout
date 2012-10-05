(function () {
    TaxiHail.AddFavoriteView = TaxiHail.TemplatedView.extend({
        events: {
            "click [data-action=save]": "save",
            "change :text[data-action=changesettings]": "onSettingsPropertyChanged",
            'focus [name=fullAddress]': 'onfocus',
            'click [data-action=destroy]': 'destroyAddress',
        },
        
        initialize :function () {
            _.bindAll(this, 'onkeyup', 'ondocumentclick');
            this.model.on('change', this.render, this);
            $(document).on('click', this.ondocumentclick);
        },

        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);
            
            this.$("#AddFavoritesForm").validate({
                rules: {
                    friendlyName: "required",
                    fullAddress: "required",
                },
                messages: {
                    friendlyName: {
                        required: TaxiHail.localize('error.friendlyNameRequired'),
                    },
                    fullAddress: {
                        required: TaxiHail.localize('error.fullAddressRequired'),
                    }
                },
                highlight: function (label) {
                    $(label).closest('.control-group').addClass('error');
                    $(label).prevAll('.valid-input').addClass('hidden');
                }, success: function (label) {
                    $(label).closest('.control-group').removeClass('error');
                    label.prevAll('.valid-input').removeClass('hidden');

                }
            });
            
            //search address for full address
            this.$('[name=fullAddress]').on('keyup', _.debounce(this.onkeyup, 500));
            this._selector = new TaxiHail.AddressSelectionView({
                model: this.model
            }).on('selected', function (model, collection) {
                this.model.set(model.toJSON());
                this.close();
            }, this);
            

            this._selector.render().hide();
            this.$(".address-selector-container").html(this._selector.el);

            return this;
        },
        
        save: function (e) {
            e.preventDefault();
            if (this.$("form").valid()) {
                if (this.model.has('fullAddress')) {
                    if (!this.model.get('isHistoric')) {
                        this.model.save();
                    } else {
                        $.post('api/account/addresses', {
                            friendlyName: this.model.get('friendlyName'),
                            fullAddress: this.model.get('fullAddress'),
                            apartment: this.model.get('apartment'),
                            ringCode: this.model.get('ringCode')
                        }, function () {
                        }, 'json');
                    }
                    this.model.trigger('reset');
                }
            }
        },
        
        destroyAddress: function (e) {
            e.preventDefault();
            TaxiHail.confirm(this.localize('Remove Favorites'),
                this.localize('modal.remove.message'),
                _.bind(function () {
                    this.model.destroy();
                }, this));
        },

        remove: function() {
            if(this._selector) this._selector.remove();
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
            this._selector && this._selector.hide();
            // Set address in textbox back to the value of the model
            this.$('[name=fullAddress]').val(this.model.get('fullAddress'));
        },

        onfocus: function (e) {
            this.open();
        },
        
        onkeyup: function (e) {
            if (!jQuery.hotkeys.specialKeys[e.which]) {
                this._selector && this._selector.search($(e.currentTarget).val());
            }
        },
        
        onSettingsPropertyChanged: function (e) {
            var $input = $(e.currentTarget);

            this.model.set($input.attr("name"), $input.val());
        },
        
        ondocumentclick: function (e) {
            if (!this.$('.address-picker').find(e.target).length) {
                this.close();
            }
        }
    });

}());