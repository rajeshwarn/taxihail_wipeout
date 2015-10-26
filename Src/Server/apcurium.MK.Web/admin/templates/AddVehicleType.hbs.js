var AddVehicleType = Handlebars.template({"1":function(depth0,helpers,partials,data) {
  return " checked ";
  },"3":function(depth0,helpers,partials,data,depths) {
  var stack1, helperMissing=helpers.helperMissing, buffer = "";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, (depth0 != null ? depth0.id : depth0), (depths[1] != null ? depths[1].referenceDataVehicleId : depths[1]), {"name":"ifCond","hash":{},"fn":this.program(4, data, depths),"inverse":this.program(6, data, depths),"data":data}));
  if (stack1 != null) { buffer += stack1; }
  return buffer;
},"4":function(depth0,helpers,partials,data) {
  var helper, functionType="function", helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression;
  return "                  <option value=\""
    + escapeExpression(((helper = (helper = helpers.id || (depth0 != null ? depth0.id : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"id","hash":{},"data":data}) : helper)))
    + "\" selected>"
    + escapeExpression(((helper = (helper = helpers.display || (depth0 != null ? depth0.display : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"display","hash":{},"data":data}) : helper)))
    + "</option>\r\n";
},"6":function(depth0,helpers,partials,data) {
  var helper, functionType="function", helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression;
  return "                  <option value=\""
    + escapeExpression(((helper = (helper = helpers.id || (depth0 != null ? depth0.id : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"id","hash":{},"data":data}) : helper)))
    + "\">"
    + escapeExpression(((helper = (helper = helpers.display || (depth0 != null ? depth0.display : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"display","hash":{},"data":data}) : helper)))
    + "</option>\r\n";
},"8":function(depth0,helpers,partials,data,depths) {
  var stack1, helperMissing=helpers.helperMissing, buffer = "";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, depth0, (depths[1] != null ? depths[1].serviceType : depths[1]), {"name":"ifCond","hash":{},"fn":this.program(9, data, depths),"inverse":this.program(11, data, depths),"data":data}));
  if (stack1 != null) { buffer += stack1; }
  return buffer;
},"9":function(depth0,helpers,partials,data) {
  var lambda=this.lambda, escapeExpression=this.escapeExpression;
  return "                <option value=\""
    + escapeExpression(lambda(depth0, depth0))
    + "\" selected>"
    + escapeExpression(lambda(depth0, depth0))
    + "</option>\r\n";
},"11":function(depth0,helpers,partials,data) {
  var lambda=this.lambda, escapeExpression=this.escapeExpression;
  return "                <option value=\""
    + escapeExpression(lambda(depth0, depth0))
    + "\">"
    + escapeExpression(lambda(depth0, depth0))
    + "</option>\r\n";
},"13":function(depth0,helpers,partials,data,depths) {
  var stack1, helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression, buffer = "        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Network vehicle type", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <select name=\"referenceNetworkVehicleTypeId\">\r\n                <option value=\"\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "None", {"name":"localize","hash":{},"data":data})))
    + "</option>\r\n";
  stack1 = helpers.each.call(depth0, (depth0 != null ? depth0.networkVehicleTypes : depth0), {"name":"each","hash":{},"fn":this.program(14, data, depths),"inverse":this.noop,"data":data});
  if (stack1 != null) { buffer += stack1; }
  return buffer + "            </select>\r\n        </div>\r\n";
},"14":function(depth0,helpers,partials,data,depths) {
  var stack1, helperMissing=helpers.helperMissing, buffer = "";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, (depth0 != null ? depth0.id : depth0), (depths[1] != null ? depths[1].referenceNetworkVehicleTypeId : depths[1]), {"name":"ifCond","hash":{},"fn":this.program(15, data, depths),"inverse":this.program(17, data, depths),"data":data}));
  if (stack1 != null) { buffer += stack1; }
  return buffer;
},"15":function(depth0,helpers,partials,data) {
  var helper, functionType="function", helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression;
  return "                    <option value=\""
    + escapeExpression(((helper = (helper = helpers.id || (depth0 != null ? depth0.id : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"id","hash":{},"data":data}) : helper)))
    + "\" selected>"
    + escapeExpression(((helper = (helper = helpers.name || (depth0 != null ? depth0.name : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"name","hash":{},"data":data}) : helper)))
    + "</option>\r\n";
},"17":function(depth0,helpers,partials,data) {
  var helper, functionType="function", helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression;
  return "                    <option value=\""
    + escapeExpression(((helper = (helper = helpers.id || (depth0 != null ? depth0.id : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"id","hash":{},"data":data}) : helper)))
    + "\">"
    + escapeExpression(((helper = (helper = helpers.name || (depth0 != null ? depth0.name : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"name","hash":{},"data":data}) : helper)))
    + "</option>\r\n";
},"19":function(depth0,helpers,partials,data) {
  return " checked";
  },"21":function(depth0,helpers,partials,data) {
  var helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression;
  return "          <a href='#' class='btn btn-danger  span' data-action='destroy'>"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Remove", {"name":"localize","hash":{},"data":data})))
    + "</a>\r\n";
},"compiler":[6,">= 2.0.0-beta.1"],"main":function(depth0,helpers,partials,data,depths) {
  var stack1, helper, helperMissing=helpers.helperMissing, escapeExpression=this.escapeExpression, functionType="function", buffer = "﻿<div class='well'>\r\n    <h4 class=\"table-title\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Add or Update A Vehicle Type", {"name":"localize","hash":{},"data":data})))
    + "</h4>\r\n\r\n    <div class=\"errors\" style=\"margin-top: 10px;\"></div>\r\n\r\n    <form class=\"form-horizontal clearfix\" >\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Name", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <div class=\"controls\">\r\n                <input type=\"text\" id=\"inputVehicleName\" name=\"name\" value=\""
    + escapeExpression(((helper = (helper = helpers.name || (depth0 != null ? depth0.name : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"name","hash":{},"data":data}) : helper)))
    + "\" class='input-block-level' maxlength=\"50\">\r\n            </div>\r\n        </div>\r\n        \r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Logo", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <div class=\"controls\">\r\n                <label class=\"radio\">\r\n                    <input class=\"radio\" type=\"radio\" id=\"inputVehicleLogoName\" name=\"logoName\" value=\"taxi\" ";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, (depth0 != null ? depth0.logoName : depth0), "taxi", {"name":"ifCond","hash":{},"fn":this.program(1, data, depths),"inverse":this.noop,"data":data}));
  if (stack1 != null) { buffer += stack1; }
  buffer += "/>\r\n                    <img src=\"assets/img/taxi_badge.png\" />\r\n                </label>\r\n                <label class=\"radio\">\r\n                    <input class=\"radio\" type=\"radio\" id=\"inputVehicleLogoName\" name=\"logoName\" value=\"suv\" ";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, (depth0 != null ? depth0.logoName : depth0), "suv", {"name":"ifCond","hash":{},"fn":this.program(1, data, depths),"inverse":this.noop,"data":data}));
  if (stack1 != null) { buffer += stack1; }
  buffer += "/>\r\n                    <img src=\"assets/img/suv_badge.png\" />\r\n                </label>\r\n                <label class=\"radio\">\r\n                    <input class=\"radio\" type=\"radio\" id=\"inputVehicleLogoName\" name=\"logoName\" value=\"blackcar\" ";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, (depth0 != null ? depth0.logoName : depth0), "blackcar", {"name":"ifCond","hash":{},"fn":this.program(1, data, depths),"inverse":this.noop,"data":data}));
  if (stack1 != null) { buffer += stack1; }
  buffer += "/>\r\n                    <img src=\"assets/img/blackcar_badge.png\" />\r\n                </label>\r\n                <label class=\"radio\">\r\n                    <input class=\"radio\" type=\"radio\" id=\"inputVehicleLogoName\" name=\"logoName\" value=\"wheelchair\" ";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, (depth0 != null ? depth0.logoName : depth0), "wheelchair", {"name":"ifCond","hash":{},"fn":this.program(1, data, depths),"inverse":this.noop,"data":data}));
  if (stack1 != null) { buffer += stack1; }
  buffer += " />\r\n                    <img src=\"assets/img/wheelchair_badge.png\" />\r\n                </label>\r\n            </div>\r\n        </div>\r\n        \r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Associated Reference Data Type", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <select name=\"referenceDataVehicleId\">\r\n";
  stack1 = helpers.each.call(depth0, (depth0 != null ? depth0.availableVehicles : depth0), {"name":"each","hash":{},"fn":this.program(3, data, depths),"inverse":this.noop,"data":data});
  if (stack1 != null) { buffer += stack1; }
  buffer += "            </select>\r\n        </div>\r\n\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Service Type", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <select name=\"serviceType\">\r\n";
  stack1 = helpers.each.call(depth0, (depth0 != null ? depth0.serviceTypes : depth0), {"name":"each","hash":{},"fn":this.program(8, data, depths),"inverse":this.noop,"data":data});
  if (stack1 != null) { buffer += stack1; }
  buffer += "            </select>\r\n        </div>\r\n\r\n";
  stack1 = ((helpers.ifCond || (depth0 && depth0.ifCond) || helperMissing).call(depth0, (depth0 != null ? depth0.isNetworkEnabled : depth0), "true", {"name":"ifCond","hash":{},"fn":this.program(13, data, depths),"inverse":this.noop,"data":data}));
  if (stack1 != null) { buffer += stack1; }
  buffer += "\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Max # Passengers", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <div class=\"controls\">\r\n                <input type=\"text\" id=\"inputVehicleCapacity\" name=\"maxNumberPassengers\" value=\""
    + escapeExpression(((helper = (helper = helpers.maxNumberPassengers || (depth0 != null ? depth0.maxNumberPassengers : depth0)) != null ? helper : helperMissing),(typeof helper === functionType ? helper.call(depth0, {"name":"maxNumberPassengers","hash":{},"data":data}) : helper)))
    + "\" class='input-block-level' maxlength=\"50\">\r\n                <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "(0 = no limit)", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            </div>            \r\n        </div>\r\n\r\n        <div class=\"control-group\">\r\n            <label class=\"control-label\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Wheelchair Accessible", {"name":"localize","hash":{},"data":data})))
    + "</label>\r\n            <div class=\"controls\">\r\n                <input type=\"checkbox\" id=\"inputIsWheelchairAccessible\" name=\"isWheelchairAccessible\" ";
  stack1 = helpers['if'].call(depth0, (depth0 != null ? depth0.isWheelchairAccessible : depth0), {"name":"if","hash":{},"fn":this.program(19, data, depths),"inverse":this.noop,"data":data});
  if (stack1 != null) { buffer += stack1; }
  buffer += ">\r\n            </div>\r\n        </div>\r\n\r\n        <div class=\"control-group btn-right-alignment\" >\r\n          <button type='submit' class='btn btn-primary span' data-loading-text=\""
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Loading", {"name":"localize","hash":{},"data":data})))
    + "\">"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Save", {"name":"localize","hash":{},"data":data})))
    + "</button>\r\n          <a href='#' class='btn  span' data-action='cancel'>"
    + escapeExpression(((helpers.localize || (depth0 && depth0.localize) || helperMissing).call(depth0, "Cancel", {"name":"localize","hash":{},"data":data})))
    + "</a>\r\n";
  stack1 = helpers.unless.call(depth0, (depth0 != null ? depth0.isNew : depth0), {"name":"unless","hash":{},"fn":this.program(21, data, depths),"inverse":this.noop,"data":data});
  if (stack1 != null) { buffer += stack1; }
  return buffer + "        </div>\r\n    </form>\r\n</div>\r\n";
},"useData":true,"useDepths":true});