(function ($) {

    function AjaxForm2(el, options) {

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

    AjaxForm2.prototype = {

        init: function () {
            var _this = this;
            $.validator.unobtrusive.parse(_this.$el);
            _this.$el.submit(function () {
                if ($(this).valid()) {
                    
                    // the GET query string
                    var formData = $(this).serialize();
                    
                    // the action will include everything that is not the file as a GET query string
                    var formAction = this.action + (formData ? "?" + formData : "");

                    var fileInputs = $("input[type=file]", $(this));
                    if (fileInputs.lenght > 0)
                        throw "There can be no more than 1 file input in the form";

                    var fileInput = fileInputs.length ? fileInputs[0] : null;
                    var fileName = fileInput ? fileInput.files[0].name : null;
                    var fileType = fileInput ? fileInput.files[0].type : null;
                    var fileSize = fileInput ? fileInput.files[0].size : null;

                    var xhr = new XMLHttpRequest();
                    xhr.open('POST', formAction);
                    xhr.setRequestHeader('Content-type', 'multipart/form-data');
                    //Appending file information in Http headers
                    xhr.setRequestHeader('X-File-Name', fileName);
                    xhr.setRequestHeader('X-File-Type', fileType);
                    xhr.setRequestHeader('X-File-Size', fileSize);
                    //Sending file in XMLHttpRequest
                    xhr.send(fileInput ? fileInput.files[0] : null);
                    xhr.onreadystatechange = function() {
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
                }
                return false;
            });
        }
    }

    $.fn.ajaxForm2 = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new AjaxForm2(this, options);
                rev.init();
                $(this).data('ajaxForm2', rev);
            });
        }
        return this;
    };

})(jQuery);