(function(){

    var View = TaxiHail.AlertView = Backbone.View.extend({

        className: 'alert clearfix',

        initialize: function(options) {
            options = options || {};

            if(options.type) {
                this.$el.addClass('alert-' + options.type);
            }
        },

        render: function() {

            this.$el.html(this.options.message);

            var $button = $('<button type="button" class="btn btn-primary pull-right">').text("OK")
                .on('click', _.bind(function(){
                    this.trigger('ok');
                },this)).appendTo(this.el);


            return this;
        }

    });  

}());