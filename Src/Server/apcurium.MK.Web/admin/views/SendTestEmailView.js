(function () {
    
    var View = TaxiHail.SendTestEmailView = TaxiHail.TemplatedView.extend({

        urlRoot: TaxiHail.parameters.apiRoot + "/admin/testemail",

        tagName: 'form',
        className: 'form-horizontal',
        
        events: {
            'click [data-action=sendtestemail]': 'sendtestemail'
        },

        render: function () {
            var data = this.model.toJSON();

            var sum = _.reduce(data, function (memo, value, key) {
                memo.push({ key: key, value: value });
                return memo;
            }, []);

            this.$el.html(this.renderTemplate({ templates: sum }));
            return this;
        },
        
        sendtestemail: function (e) {
            e.preventDefault();
            var email = this.$('[name=email]').val();
            var templateName = this.$('[name=templateName]').val();
           
            return $.ajax({
                type: 'POST',
                url: this.urlRoot + '/' + email,
                data: {
                    templateName: templateName
                },
                dataType: 'json',
                success: _.bind(function () {
                    this.$('.errors').text(TaxiHail.localize('sendTestEmailSuccess'));
                }, this)
            }).fail(_.bind(function (xhr, textStatus, error) {
                this.$('.errors').text(TaxiHail.localize(xhr.statusText));
            }), this)
                .always(this.$(':submit').button('reset'));
        }
    });
}());