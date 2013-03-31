(function($) {
    // Allows for creating a dropdown-menu out of any element in the page. Anything can become a dropdown menu.
    function CameraWrapper(el, options) {

        // Defaults:
        this.defaults = {
            postUrl: "",
            $cameraContainer: null
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);
    }
    
    // Separate functionality from object creation
    CameraWrapper.prototype = {
        init: function() {
            var _this = this;
            
            var pos = 0, ctx = null, saveCb, image = [];

            var canvas = document.createElement("canvas");
            canvas.setAttribute('width', 320);
            canvas.setAttribute('height', 240);

            if (canvas.toDataURL) {
                ctx = canvas.getContext("2d");
                image = ctx.getImageData(0, 0, 320, 240);
                saveCb = function (data) {

                    var col = data.split(";");
                    var img = image;

                    for (var i = 0; i < 320; i++) {
                        var tmp = parseInt(col[i]);
                        img.data[pos + 0] = (tmp >> 16) & 0xff;
                        img.data[pos + 1] = (tmp >> 8) & 0xff;
                        img.data[pos + 2] = tmp & 0xff;
                        img.data[pos + 3] = 0xff;
                        pos += 4;
                    }

                    if (pos >= 4 * 320 * 240) {
                        ctx.putImageData(img, 0, 0);
                        $.post(_this.opts.postUrl, { type: "data", image: canvas.toDataURL("image/png") });
                        pos = 0;
                    }
                };

            } else {
                saveCb = function (data) {
                    image.push(data);
                    pos += 4 * 320;
                    if (pos >= 4 * 320 * 240) {
                        $.post(_this.opts.postUrl, { type: "pixel", image: image.join('|') });
                        pos = 0;
                    }
                };
            }

            _this.opts.$cameraContainer.click(function () {
                e.preventDefault();
                window.webcam.capture();
            });
        }
    };

    // The actual plugin
    $.fn.cameraWrapper = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new CameraWrapper(this, options);
                rev.init();
                $(this).data('cameraWrapper', rev);
            });
        }
        return this;
    };
})(jQuery);