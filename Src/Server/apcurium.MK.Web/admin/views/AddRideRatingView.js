(function () {

    var View = TaxiHail.AddRideRatingView = TaxiHail.TemplatedView.extend({

        className: 'add-riderating-view',

        events: {
            'click [data-action=destroy]': 'destroyRating',
            'click [data-action=cancel]': 'cancel'
        },

        initialize: function () {
            _.bindAll(this, 'save');
        },

        render: function () {

            var data = _.extend(this.model.toJSON(), {
                isNew: this.model.isNew(),
                languages: TaxiHail.parameters.languages
            });

            var html = this.renderTemplate(data);
            this.$el.html(html);

            this.validate({
                rules: {
                    ratingFields0: {
                        require_from_group: [1, ".input-block-level"]
                    },
                    ratingFields1: {
                        require_from_group: [1, ".input-block-level"]
                    },
                    ratingFields2: {
                        require_from_group: [1, ".input-block-level"]
                    }
                },
                submitHandler: this.save
            });

            return this;
        },

        save: function (form) {
            var ratings = this.serializeForm(form);

            this.model.save(ratings, {
                success: _.bind(function (model) {
                    var displayName = '';
                    var ratingTypes = this.model.attributes.ratingTypes;
                    var englishLocalization = ratingTypes[1].name;

                    if (englishLocalization) {
                        displayName = englishLocalization;
                    } else {
                        // Take first localized string that we find
                        for (i = 0; i < ratingTypes.length; i++) {
                            if (ratingTypes[i].name) {
                                displayName = ratingTypes[i].name;
                                break;
                            }
                        }    
                    }

                    var namedModel = _.extend(this.model.toJSON(),
                    {
                        name: displayName
                    });

                    this.collection.add(namedModel);
                    TaxiHail.app.navigate('ratings', { trigger: true });

                }, this),
                error: function (model, xhr, options) {
                    this.$(':submit').button('reset');

                    var alert = new TaxiHail.AlertView({
                        message: TaxiHail.localize(xhr.statusText),
                        type: 'error'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.errors').html(alert.render().el);
                }
            });
        },

        destroyRating: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('Remove Rating'),
                message: this.localize('modal.removeRating.message')
            }).on('ok', function () {
                this.model.destroy({ url: TaxiHail.parameters.apiRoot + '/ratingtypes/' + this.model.get('id') });
                TaxiHail.app.navigate('ratings', { trigger: true });
            }, this);
        },

        remove: function () {
            this.$el.remove();
            return this;
        },

        cancel: function (e) {
            e.preventDefault();
            this.model.set(this.model.previousAttributes);
            this.trigger('cancel', this);
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());