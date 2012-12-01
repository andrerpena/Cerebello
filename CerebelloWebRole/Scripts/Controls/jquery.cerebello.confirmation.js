(function ($) {

    // creates a screen to ask the user for confirmation
    function Confirmation() {

        // Defaults:
        this.defaults = {

            // Title of the window... default is the same as the operationName.
            title: "{operationName}",

            // Text with informations about the risks of the operation, and other details about the operation.
            message: function () {
                return ("OPERATION: {operationName}<br />"
                    + "OBJECT TYPE: {objectType}"
                    + "OBJECT NAME: {objectName}");
            },

            // Text instructing the user on how to process with the operation.
            confirmMessage: function () {
                if (this.checkString)
                    return "TYPE: '{checkString}'.";
                return "CLICK THE BUTTON.";
            },

            // String that the user must type, so that the operation is executed.
            checkString: null,

            // The error message displayed when the server returns an error.
            errorMessage: "FAILED: {operationName}.",

            // The text used between the action button and the cancel link. e.g. "ou".
            orText: "|",

            // The cancel text used in the cancel link. e.g. "cancelar".
            cancelText: "CANCEL",

            // The action name used in the button. e.g. "resetar senha".
            actionName: "OK",

            // The operation name. e.g. "resetar senha".
            operationName: null,

            // The URL used to call the server side operation.
            url: null,

            // Optional. The type of the object affected by the operation. Like 'usuário'.
            objectType: null,

            // Optional. The name of the specific object affected by the operation. Like 'Jonas da Silva'.
            objectName: null,

            // This can be either a function or an URL.
            success: null,

            // This field is going to be processed twice.
            // The first time it is formatted with 'options' object. Fields are like this '{field}'.
            // The second time it is formatted with 'data' object from json response. Fields are like this '{{field}}'
            successMessage: "SUCCESS!",

            // This field is going to be processed twice.
            // The first time it is formatted with 'options' object. Fields are like this '{field}'.
            // The second time it is formatted with 'data' object from json response. Fields are like this '{{field}}'
            techInfoText: function () {
                // Why this function returns another function:
                // REMEMBER: this is going to be called twice, once for 'options' and then for 'data'.
                return function () { return "MORE INFO: " + this.text; };
            },

        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, cerebello.res.confirmationBase);

        for (var i = 0, len = arguments.length; i < len; i++)
            this.opts = $.extend(this.opts, arguments[i]);
    }

    // Separate functionality from object creation
    Confirmation.prototype = {

        init: function () {
            var _this = this;

            // formatter function:
            // s: string to be formatted, it may contain fields in the form "{name}", where name is a property of the o object.
            // o: object containing the properties that are used as replacements of "{name}" fields in the text.
            // Note: 's' can be a function that returns a string that will be formatted, or anything else.
            // Note: if 's' is not a string, and is not a function, then the 's' will be returned without formatting.
            // Note: the used properties of 'o' are formatted recursivelly, until no more replacement fields are found.
            var fmt = function (s, o) {
                if (typeof o == 'undefined')
                    return undefined;

                // If s is a function call it, so it returns a string,
                // or anything that will be used latter.
                if (typeof s == 'function')
                    s = s.apply(o);

                // Replace the fields inside text, only if it is a string.
                if (typeof s == 'string') {
                    s = s.replace(/{{|}}|{([^}]+)}/g, function (match, propName) {
                        if (match == "{{" || match == "}}") return match[0];
                        return typeof o[propName] != 'undefined'
                            ? "" + fmt(o[propName], o) // recursivelly replacing formatting fields
                            : match;
                    });
                }

                return s;
            };

            // formatting all incoming strings
            var title = fmt(_this.opts.title, _this.opts);
            var message = fmt(_this.opts.message, _this.opts);
            var confirmMessage = fmt(_this.opts.confirmMessage, _this.opts);
            var checkString = fmt(_this.opts.checkString, _this.opts);
            var errorMessage = fmt(_this.opts.errorMessage, _this.opts);
            var orText = fmt(_this.opts.orText, _this.opts);
            var cancelText = fmt(_this.opts.cancelText, _this.opts);
            var actionName = fmt(_this.opts.actionName, _this.opts);
            var url = fmt(_this.opts.url, _this.opts);
            var techInfoText = fmt(_this.opts.techInfoText, _this.opts);
            var operationName = fmt(_this.opts.operationName, _this.opts);
            var successMessage = fmt(_this.opts.successMessage, _this.opts);

            // checking input parameters
            if (typeof _this.opts.success != 'function') throw "The success function should be set, and must be a function.";
            if (!title) throw "The title property should be set.";
            if (!message) throw "The message property should be set.";
            if (!cancelText) throw "The cancelText property should be set.";
            if (!actionName) throw "The actionName property should be set.";
            if (!operationName) throw "The operationName property should be set.";
            if (!url) throw "The url property should be set.";

            // opening modal window
            $.modal({
                title: title,
                buildContent: function ($content) {
                    // adds the red title box
                    $("<div/>")
                        .addClass("confirmation-warning")
                        .html(message)
                        .appendTo($content);

                    $("<p/>")
                        .addClass("confirmation-title")
                        .html(confirmMessage)
                        .appendTo($content);

                    // text
                    var $inputText = null;
                    if (checkString) {
                        $inputText = $("<input/>")
                            .attr("type", "text")
                            .addClass("confirmation-text")
                            .appendTo($content);
                    }

                    // submit-bar
                    var $submitBar = $("<div/>")
                        .addClass("submit-bar")
                        .appendTo($content);

                    var $inputSubmit = $("<input/>")
                        .attr("type", "submit")
                        .attr("value", actionName)
                        .appendTo($submitBar);

                    if (checkString)
                        $inputSubmit.attr("disabled", "disabled");

                    $("<span/>")
                        .addClass("separator")
                        .text(orText)
                        .appendTo($submitBar);

                    var $linkCancel = $("<a/>")
                        .attr("href", "#")
                        .text(cancelText)
                        .appendTo($submitBar);

                    // will enable the submit button as soon as the correct checkString is typed
                    if ($inputText) {
                        $inputText.bind("keyup", function () {
                            if ($inputText.val() == checkString)
                                $inputSubmit.removeAttr("disabled");
                            else
                                $inputSubmit.attr("disabled", "disabled");
                        });
                    }

                    // handles the cancel button
                    $linkCancel.click(function (e) {
                        e.preventDefault();
                        $(this).trigger("modal-cancel");
                    });

                    // handles the submit button
                    $inputSubmit.click(function (e) {
                        e.preventDefault();

                        $("input, textarea, select", $content).attr("disabled", "disabled");
                        var sendData = {};
                        if ($inputText) sendData.typeTextCheck = $inputText.val();
                        $.getJSON(url, sendData, function (data) {
                            if (data.success) {
                                if (successMessage)
                                    $.notify(fmt(successMessage, data));
                                _this.opts.success(data);
                            }
                            else {
                                $.notify($.trim(errorMessage + "\n" + fmt(techInfoText, data)));
                            }
                        });
                    });

                    if ($inputText)
                        $inputText.focus();
                },
                // sets the height auto
                height: null
            });
        }
    };

    // The actual plugin
    $.confirmation = function () {
        // calling constructor of Confirmation using apply, to pass arguments
        var __Confirmation = function () { };
        __Confirmation.prototype = Confirmation.prototype;
        var confirmation = new __Confirmation();
        Confirmation.apply(confirmation, arguments);

        confirmation.init();

        return confirmation;
    };

    // Sample of how to do other plugins based on the $.confirmation
    // This may be removed if we find that it is better to always pass
    // cerebello.res.deleteConfirmation when we want to delete something.
    $.deleteConfirmation2 = function () {
        var a = [cerebello.res.deleteConfirmation];
        a.push.apply(a, arguments);

        return $.confirmation.apply($.confirmation, a);
    };
})(jQuery);
