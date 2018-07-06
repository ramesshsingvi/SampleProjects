/**
 * bootstrap-checkbox.js
 * (c) 2013~ Jiung Kang
 * Licensed under the Apache License, Version 2.0 (the "License");
 */

(function($) {
  "use strict";

  var replaceCheckboxElement = function(checkbox, element) {
      var value = element.val(),
          id = element.attr('id'),
          name = element.attr('name'),
          className = element.attr('class'),
          style = element.attr('style'),
          checked = !!element[0].checked,
          status = element.data('status');

      var welNew = $('<span><div></div><input name=' + name + '  id=' + id + ' type="hidden" value=""></span>');

    if (status.toUpperCase() == 'TRUE')
    {
        checked = true;
    }
    else if (status.toUpperCase() == 'FALSE') {
        checked = false;
    } else if (checked == false) {
        checked = null;
    }

    element.replaceWith(welNew);
    var innerDiv = welNew.find('div');

    if (id) { innerDiv.attr('id', id) }
    if (className) { innerDiv.attr('class', className) }
    innerDiv.addClass('bootstrap-checkbox');
    if (style) { innerDiv.attr('style', style); }
    if (checked) { innerDiv.addClass('checked'); }

    checkbox.value = value;
    checkbox.checked = checked;
    checkbox.ambiguous = true;
    checkbox.element = welNew;
  };

  var changeCheckView = function (element, checked) {
      var innerDiv = element.find('div');
      var input = element.find('input');

    innerDiv.removeClass('ambiguous');
    innerDiv.removeClass('checked');

    if (checked === null) {
        innerDiv.addClass('ambiguous');
        innerDiv.html('<i class="fa fa-question fa-2x"></i>');
        input.val(undefined);
    } else if (checked) {
        innerDiv.addClass('checked');
        innerDiv.html('<i class="fa fa-thumbs-up fa-2x"></i>');
        input.val('true');
    } else {
        innerDiv.html('');
        innerDiv.html('<i class="fa fa-thumbs-down fa-2x"></i>');
        input.val('false');
    }
  };

  var attachEvent = function (checkbox, element) {
    element.on('click', function(e) {
      var checked;
      if (checkbox.checked) {
        checked = false;
      } else if (checkbox.checked === false && checkbox.ambiguous === true){
        checked = null;
      } else {
        checked = true;
      }

      checkbox.checked = checked;
      changeCheckView(checkbox.element, checked);

      checkbox.element.trigger({
        type: 'check',
        value: checkbox.value,
        checked: checked,
        element: checkbox.element
      });
    });

    element.ready(function () {

        changeCheckView(checkbox.element, checkbox.checked);
  });
};


  var Checkbox = function(element, options) {
    replaceCheckboxElement(this, element);
    attachEvent(this, this.element.find('div'));
    if (options && options.label) {
      attachEvent(this, $(options.label));
    }

    //changeCheckView(element, true);
  };

  $.fn.extend({
    checkbox : function(options) {
      var aReplaced = $(this.map(function () {
        var $this = $(this),
            checkbox = $this.data('checkbox');

        if (!checkbox) {
          checkbox = new Checkbox($this, options);
          checkbox.element.data('checkbox', checkbox);
        }

        return checkbox.element[0];
      }));
      changeCheckView(aReplaced, false);
      aReplaced.selector = this.selector;
      return aReplaced;
    },

    chbxVal : function(value) {
      var $this = $(this[0]);
      var checkbox = $this.data('checkbox');

      if (!checkbox) {
        return;
      }
      if ($.type(value) === "undefined") {
        return checkbox.value;
      } else {
        checkbox.value = value;
        $this.data('checkbox', checkbox);
      }
    },

    chbxChecked : function(checked) {
      var $this = $(this[0]);
      var checkbox = $this.data('checkbox');

      if (!checkbox) {
        return;
      }
      if ($.type(checked) === "undefined") {
        return checkbox.checked;
      } else {
        checked === null;
        changeCheckView($this, checked);

        checkbox.checked = checked;
        $this.data('checkbox', checkbox);
      }
    }
  });
})(jQuery);