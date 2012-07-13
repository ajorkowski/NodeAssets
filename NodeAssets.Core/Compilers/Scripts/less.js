var less = require('less');

/*var workingDir = '{0}';

var opt = {};
if(workingDir != '')
{
    opt.filename = workingDir;
}*/

var stdin = process.openStdin();
stdin.setEncoding('utf8');

var styl = '';
stdin.on('data', function (chunk) {
    styl = styl + chunk;
});

stdin.on('end', function () {
    less.render(styl, function (e, css) {
        if (e) throw e;
        console.log(css);
    });
});