var less = require('less');

var opt = {0};

var stdin = process.openStdin();
stdin.setEncoding('utf8');

var styl = '';
stdin.on('data', function (chunk) {
    styl = styl + chunk;
});

stdin.on('end', function () {
    less.render(styl, opt, function (e, css) {
        if (e) { console.log(e); return; }
        console.log(JSON.stringify(css));
    });
});