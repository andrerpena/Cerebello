(function ($) {

    // creates a screen to ask the user for deletion confirmation
    function DeleteConfirmation(options) {

        // Defaults:
        this.defaults = {
            objectType: null,
            // optional. The name of the specific object being deleted. Like 'Jonas da Silva'
            objectName: null,
            // this can be either a function or an URL
            success: null
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = null;

        // this.$prevButton = null;
    }

    // Separate functionality from object creation
    DeleteConfirmation.prototype = {

        init: function () {
            var _this = this;

            $.modal({
                title: "Excluir " + (_this.opts.objectName ?  _this.opts.objectName : _this.opts.objectType ) + "?",
                buildContent: function ($content) {

                    // adds the red title box
                    $("<div/>").addClass("delete-confirmation-warning").html("Você está tentando excluir um(a) <b>"
                    + _this.opts.objectType + "</b> permanentemente" + 
                    
                    // optionally adds object name descriptions if it exists
                    (_this.opts.objectName ? " <b>(" + _this.opts.objectName + ")</b>" : "")
                    
                     + ". Todos os registros que dependem deste podem ser excluídos também. Esta operação não pode ser desfeita.")
                        .appendTo($content);

                    $("<p/>").addClass("delete-confirmation-title").html("Para prosseguir com a exclusão, digite <b>"
                    + _this.opts.objectType + "</b> no campo abaixo").appendTo($content);

                    // text
                    var $inputText = $("<input/>").attr("type", "text").addClass("delete-confirmation-text").appendTo($content);

                    // submit-bar
                    var $submitBar = $("<div/>").addClass("submit-bar").appendTo($content);
                    var $inputSubmit = $("<input/>").attr("type", "submit").attr("disabled", "disabled").attr("value", "excluir").appendTo($submitBar);

                    $("<span/>").addClass("separator").text("ou").appendTo($submitBar);
                    var $linkCancel = $("<a/>").attr("href", "#").text("cancelar").appendTo($submitBar);

                    // will enable the submit button as soon as the correct objectType is typed
                    $inputText.bind("keyup", function (e) {
                        if ($inputText.val() == _this.opts.objectType)
                            $inputSubmit.removeAttr("disabled");
                        else
                            $inputSubmit.attr("disabled", "disabled");
                    });

                    // handles the cancel button
                    $linkCancel.click(function (e) {
                        e.preventDefault();
                        $(this).trigger("modal-cancel");
                    });

                    // handles the submit button
                    $inputSubmit.click(function (e) {
                        e.preventDefault();

                        if (!_this.opts.success)
                            throw "The success function should be set.";

                        if (!_this.opts.url)
                            throw "The url property should be set.";

                        $("input, textarea, select", $content).attr("disabled", "disabled");

                        $.getJSON(_this.opts.url, function (data) {
                            if (data.success) {
                                _this.opts.success(data);
                            }
                            else {
                                alert("Não foi possível excluir este(a) '" + _this.opts.objectType + "'.\n" + "Informações técnicas: " + data.text);
                            }
                        });
                    });

                    $inputText.focus();
                },
                // sets the height auto
                height: null
            });
        }
    };

    // The actual plugin
    $.deleteConfirmation = function (options) {
        var deleteConfirmation = new DeleteConfirmation(options);
        deleteConfirmation.init();

        return deleteConfirmation;
    };
})(jQuery);
