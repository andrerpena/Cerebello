// generates a new guid
function generateGuid(separator) {
    if (!separator)
        separator = "-";
    var hunk = function () {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (hunk() + hunk() + separator + hunk() + separator + hunk() + separator + hunk() + separator + hunk() + hunk() + hunk());
}



// gets the cep
function getGetInfo(url, cep, opts) {
    if (!cep)
        throw "O CEP precisa estar preenchido";
    else
        $.ajax({
            url: url,
            data: { cep: cep },
            dataType: "json",
            success: function (e) {
                var cepInfo = e ? e : {};
                opts.success(cepInfo);
            },
            error: function () {
                opts.error();
                alert("Não foi possível obter o endereço. Ou o CEP não é válido ou não foi possível consultar o site dos Correios.");
            }
        });
}