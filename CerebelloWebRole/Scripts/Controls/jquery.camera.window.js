(function ($) {
    // Allows for creating a dropdown-menu out of any element in the page. Anything can become a dropdown menu.
    function CameraWindow(el, options) {

        // Defaults:
        this.defaults = {
            success: function (data) {}
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);
    }

    // Separate functionality from object creation
    CameraWindow.prototype = {
        init: function () {
            var _this = this;

            if (!window.cameraWindowGetUrl)
                throw "window.cameraWindowGetUrl should be globally declared";

            if (!window.cameraWindowPostUrl)
                throw "window.cameraWindowPostUrl should be globally declared";
            
            window.appointmentModal = $.modal({
                url: window.cameraWindowGetUrl,
                title: "Capturar foto",
                width: 340,
                height: 430,
                ok: function (data) {
                    _this.opts.success(data);
                }
            });
        }
    };

    // The actual plugin
    $.cameraWindow = function (options) {
        var rev = new CameraWindow(this, options);
        rev.init();
        return rev;
    };
})(jQuery);