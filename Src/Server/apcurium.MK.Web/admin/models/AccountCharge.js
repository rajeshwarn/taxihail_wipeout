(function () {
    
    var methodMap = {
        'create': 'POST',
        'update': 'PUT',
        'delete': 'DELETE',
        'read': 'GET'
    };
    
    // Helper function to get a value from a Backbone object as a property
    // or as a function.
    var getValue = function (object, prop) {
        if (!(object && object[prop])) return null;
        return _.isFunction(object[prop]) ? object[prop]() : object[prop];
    };

    // Throw an error when a URL is needed, and none is supplied.
    var urlError = function () {
        throw new Error('A "url" property or function must be specified');
    };

    var AccountCharge = TaxiHail.AccountCharge = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/accountscharge',
        sync: function (method, model, options) {
            
        var type = methodMap[method];

        // Default options, unless specified.
        options || (options = {});

        // Default JSON-request options.
        var params = { type: type, dataType: 'json' };
        params.url = TaxiHail.parameters.apiRoot + '/admin/accountscharge';

        // Ensure that we have the appropriate request data.
        if (!options.data && model && (method == 'create' || method == 'update')) {
            params.contentType = 'application/json';

            var arrayLength = model.get('questionFields').length;
            model.set('questions', []);
            for (var i = 0; i < arrayLength; i++) {
                model.get('questions')[i] = {
                    question: model.get('questionFields')[i],
                    isRequired: model.get('questionIsRequiredFields' + i) == 'on'
                };
            }

            params.data = JSON.stringify(model);
        }

        // For older servers, emulate JSON by encoding the request into an HTML-form.
        if (Backbone.emulateJSON) {
            params.contentType = 'application/x-www-form-urlencoded';
            params.data = params.data ? {model: params.data} : {};
        }

        // For older servers, emulate HTTP by mimicking the HTTP method with `_method`
        // And an `X-HTTP-Method-Override` header.
        if (Backbone.emulateHTTP) {
            if (type === 'PUT' || type === 'DELETE') {
                if (Backbone.emulateJSON) params.data._method = type;
                params.type = 'POST';
                params.beforeSend = function(xhr) {
                    xhr.setRequestHeader('X-HTTP-Method-Override', type);
                };
            }
        }

        // Don't process data on a non-GET request.
        if (params.type !== 'GET' && !Backbone.emulateJSON) {
            params.processData = false;
        }

        // Make the request, allowing the user to override any Ajax options.
        return $.ajax(_.extend(params, options));
    }
        
   });

}());