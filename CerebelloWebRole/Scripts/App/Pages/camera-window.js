function setupCamera(callback) {

    if (!window.cameraWindowGetUrl)
        throw "window.cameraWindowGetUrl should be globally declared";

    if (!window.cameraWindowPostUrl)
        throw "window.cameraWindowPostUrl should be globally declared";

    var pos = 0, ctx = null, saveCb, image = [];

    var canvas = document.createElement("canvas");
    canvas.setAttribute('width', 320);
    canvas.setAttribute('height', 240);

    function postData(image) {
        $.post(window.cameraWindowPostUrl, {
            type: "data",
            image: image
        },
        function(data) {
            if (!data || !data.success)
                alert("Ocorreu um erro enviando a imagem");
            callback(data);
        });
    };

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
                postData(canvas.toDataURL("image/png"));
                pos = 0;
            }
        };

    } else {

        saveCb = function (data) {
            image.push(data);

            pos += 4 * 320;

            if (pos >= 4 * 320 * 240) {
                postData(image.join('|'));
                pos = 0;
            }
        };
    }

    $("#camera").webcam({

        width: 320,
        height: 240,
        mode: "callback",
        swffile: "/content/camera/jscam_canvas_only.swf",

        onSave: saveCb,

        onCapture: function () {
            $("#take-picture").toggle();
            $("#processing-camera").toggle();
            webcam.save();
        },

        debug: function (type, string) {
            if (string == "Camera started") {
                $("#unavailable-camera").toggle();
                $("#take-picture").toggle();
            }
            console.log(type + ": " + string);
        }
    });

    $("#take-picture").click(function (e) {
        e.preventDefault();
        window.webcam.capture();
    });
}
