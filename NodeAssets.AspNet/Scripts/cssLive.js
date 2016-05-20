$(function () {
    var connection = $.connection('/{0}');

    var sequence = Math.floor(Math.random() * 1000000000001);
    connection.received(function (data) {
        var url = data.css;
        if (url.indexOf("?v=") == -1) {
            url = url + '?v=' + sequence;
            sequence++;
        }
        var el = document.getElementById(data.id);
        if (el) {
            el.href = url;
        }
    });

    connection.start();
});