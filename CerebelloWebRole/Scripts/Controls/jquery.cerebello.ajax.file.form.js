(function ($) {

    function AjaxFileForm(el, options) {

        //Defaults:
        this.defaults = {
            success: function () { },
            error: function () { }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);
    }

    AjaxFileForm.prototype = {
        init: function () {
            var _this = this;
            $.validator.unobtrusive.parse(_this.$el);
            _this.$el.submit(function () {
                if ($(this).valid()) {

                    // browser supports xhr2 (that is html5)
                    // creating and filling FormData object
                    var formData = new FormData();
                    var data1 = $(this).serializeArray();
                    for (var itInput = 0; itInput < data1.length; itInput++) {
                        formData.append(data1[itInput].name, data1[itInput].value);
                    }
                    var fileInputs = $("input[type=file]", $(this));

                    for (var itFileInput = 0; itFileInput < fileInputs.length; itFileInput++) {
                        var fileInput = fileInputs[itFileInput];
                        for (var itFile = 0; itFile < fileInput.files.length; itFile++) {
                            var file = fileInput.files[itFileInput];
                            formData.append($(fileInput).attr("name"), file);
                        }
                    }

                    // sending request
                    var xhr = new XMLHttpRequest();
                    xhr.open('POST', this.action);

                    xhr.onreadystatechange = function () {
                        if (xhr.readyState == 4 && xhr.status == 200) {
                            var contentTypeHeader = xhr.getResponseHeader("Content-Type");
                            var data = xhr.response;
                            var contentType;
                            if (contentTypeHeader.indexOf("text/html") != -1)
                                contentType = "html";
                            else if (contentTypeHeader.indexOf("application/json") != -1) {
                                contentType = "json";
                                data = eval("(" + xhr.response + ")");
                            } else
                                contentType = "application/octet-stream";

                            _this.opts.success.call(_this.$el, data, contentType);
                        } else {
                            _this.opts.error.call(_this.$el);
                        }
                    };

                    xhr.send(formData);
                }

                return false;
            });
        }
    };

    $.fn.ajaxFileForm = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new AjaxFileForm(this, options);
                rev.init();
                $(this).data('ajaxFileForm', rev);
            });
        }
        return this;
    };

})(jQuery);