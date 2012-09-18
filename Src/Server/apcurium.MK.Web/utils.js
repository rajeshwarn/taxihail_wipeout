(function () {

    _.extend(TaxiHail, {

        localize: function (resourceName, resourceSet) {
            var resource = '';
            if (resourceSet && !_.isUndefined(TaxiHail.resources[resourceSet])) {
                resource = TaxiHail.resources[resourceSet][resourceName];
            }
            return resource || TaxiHail.resources.Global[resourceName] || '[' + resourceName + ']';
        },

        addResourceSet: function (name, resourceSet) {
            TaxiHail.resources[name] = resourceSet;
        },

        postpone: function (func, context) {
            return _.debounce(_.bind(func, context), 800);
        },

    });

    Handlebars.registerHelper('localize', function (resourceName) {
        return new Handlebars.SafeString(TaxiHail.localize(resourceName, this.resourceSet));
    });


}());