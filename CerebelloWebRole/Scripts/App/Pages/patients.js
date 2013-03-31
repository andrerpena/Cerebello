$(function() {
    $("#alter-picture").click(function(e) {
        e.preventDefault();
        $.modal({
            url: "/p/consultoriodrhouse/d/gregoryhouse/Schedule/Create?findNextAvailable=True",
            title: "Nova consulta",
            data: {
                date: "2013-03-30",
                start: "",
                end: "",
                doctorId: ""
            },
            width: 480,
            height: 200
        });
    });
})