// generates a new guid
function generateGuid(separator) {
    if (!separator)
        separator = "-";
    var hunk = function () {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (hunk() + hunk() + separator + hunk() + separator + hunk() + separator + hunk() + separator + hunk() + hunk() + hunk());
}

window.onerror = function(error) {
    $.notify("Ocorreu um erro: " + error, null, 8000);
}