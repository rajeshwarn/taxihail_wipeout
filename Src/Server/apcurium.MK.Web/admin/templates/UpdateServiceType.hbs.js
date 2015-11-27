var UpdateServiceType = Handlebars.template({"compiler":[6,">= 2.0.0-beta.1"],"main":function(depth0,helpers,partials,data) {
  var helper, helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression, functionType="function";
  return "﻿<div class='well'>\r\n    <h4 class=\"table-title\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Update Service Type Settings", {"name":"localize","hash":{},"data":data})))
    + "</h4>\r\n\r\n    <div class=\"errors\" style=\"margin-top: 10px;\"></div>\r\n\r\n    <form class=\"form-horizontal clearfix\">\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Service Type", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <div class=\"controls\">\r\n                <label class=\"control-label\">"
    + escapeExpression(((helper = (helper = helpers.serviceType || (depth0 != null ? depth0.serviceType : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"serviceType","hash":{},"data":data}) : helper)))
    + "</label>\r\n            </div>\r\n        </div>\r\n\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "IBS - WebServicesUrl", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <div class=\"controls\">\r\n                <input type=\"text\" id=\"inputIbsWebServicesUrl\" name=\"ibsWebServicesUrl\" value=\""
    + escapeExpression(((helper = (helper = helpers.ibsWebServicesUrl || (depth0 != null ? depth0.ibsWebServicesUrl : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"ibsWebServicesUrl","hash":{},"data":data}) : helper)))
    + "\" class='input-block-level'>\r\n            </div>\r\n        </div>\r\n\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Provider ID", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <div class=\"controls\">\r\n                <input type=\"text\" id=\"inputProviderId\" name=\"providerId\" value=\""
    + escapeExpression(((helper = (helper = helpers.providerId || (depth0 != null ? depth0.providerId : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"providerId","hash":{},"data":data}) : helper)))
    + "\" class='input-block-level'>\r\n            </div>\r\n        </div>\r\n\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Wait Time Rate (Per minute)", {"name":"localize","hash":{},"data":data})))
    + "></label>\r\n                <div class=\"controls\">\r\n                    <input type=\"number\" id=\"inputWaitTimeRatePerMinute\" name=\"waitTimeRatePerMinute\" value=\""
    + escapeExpression(((helper = (helper = helpers.waitTimeRatePerMinute || (depth0 != null ? depth0.waitTimeRatePerMinute : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"waitTimeRatePerMinute","hash":{},"data":data}) : helper)))
    + "\" class='input-block-level' maxlength=\"50\">\r\n                </div>\r\n            </div>\r\n\r\n            <div class=\"control-group\">\r\n                <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Airport Meet & Greet Rate", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n                <div class=\"controls\">\r\n                    <input type=\"number\" id=\"inputAirportMeetAndGreetRate\" name=\"airportMeetAndGreetRate\" value=\""
    + escapeExpression(((helper = (helper = helpers.airportMeetAndGreetRate || (depth0 != null ? depth0.airportMeetAndGreetRate : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"airportMeetAndGreetRate","hash":{},"data":data}) : helper)))
    + "\" class='input-block-level' maxlength=\"50\">\r\n                </div>\r\n            </div>\r\n\r\n            <div class=\"control-group btn-right-alignment\">\r\n                <button type='submit' class='btn btn-primary span' data-loading-text=\""
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Loading", {"name":"localize","hash":{},"data":data})))
    + "\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Save", {"name":"localize","hash":{},"data":data})))
    + "</button>\r\n                <a href='#' class='btn  span' data-action='cancel'>"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Cancel", {"name":"localize","hash":{},"data":data})))
    + "</a>\r\n            </div>\r\n    </form>\r\n</div>\r\n";
},"useData":true});