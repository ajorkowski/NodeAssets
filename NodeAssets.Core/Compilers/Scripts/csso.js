var csso = require('csso');

var stdin = process.openStdin();
stdin.setEncoding('utf8');

var css = '';
stdin.on('data', function (chunk) {
    css = css + chunk;
});

stdin.on('end', function () {
    var extract = csso.justDoIt(css);

    console.log(extract);
});

