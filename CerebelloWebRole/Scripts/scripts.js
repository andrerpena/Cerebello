function createCookie(name, value, days) {
    var expires;
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
    }
    else {
        expires = "";
    }
    document.cookie = name + "=" + value + expires + "; path=/";
}

function readCookie(name) {
    var nameEq = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEq) == 0) return c.substring(nameEq.length, c.length);
    }
    return null;
}

function eraseCookie(name) {
    createCookie(name, "", -1);
}

// generates a new guid
function generateGuid(separator) {
    if (!separator)
        separator = "-";
    var hunk = function () {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (hunk() + hunk() + separator + hunk() + separator + hunk() + separator + hunk() + separator + hunk() + hunk() + hunk());
}

window.ajaxLoad = function(url, placeHolderSelector, behavior) {
    if (!behavior)
        behavior = "replace";

    $.ajax({
        url: url,
        success: function (result) {
            var $placeHolder = placeHolderSelector instanceof jQuery ? placeHolderSelector : $(placeHolderSelector);
            switch (behavior) {
                case "replace":
                    $placeHolder.replaceWith(result);
                    break;
                case "fill":
                    $placeHolder.html(result);
                    break;
                default:
                    throw "unsupported behavior";
            }
        }
    });
    return false;
}