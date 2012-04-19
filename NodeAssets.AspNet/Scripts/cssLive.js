$(function () {
    var connection = $.connection('/{0}');

    var sequence = 0;
    connection.received(function (data) {
        var url = data.css + '?v=' + sequence;
        document.getElementById(data.id).href = url;
    });

    connection.start();
});