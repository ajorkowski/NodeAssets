var stylus = require('stylus');

var useNib = {0};
var workingDir = '{1}';

var opt = {};
if(workingDir != '')
{
    opt.filename = workingDir;
}

var stdin = process.openStdin();
stdin.setEncoding('utf8');

var styl = '';
stdin.on('data', function (chunk) {
    styl = styl + chunk;
});

stdin.on('end', function () {
    var working = stylus(styl, opt);

    if(useNib) {
        working.use(require('nib')());
        working.import('nib');
    }

    working.render(function(err, css) {
        if(err) throw err;
        console.log(css);
    });
});