var jsp = require("uglify-js").parser;
var pro = require("uglify-js").uglify;

var stdin = process.openStdin();
stdin.setEncoding('utf8');

var orig_code = '';
stdin.on('data', function (chunk) {
    orig_code = orig_code + chunk;
});

stdin.on('end', function () {
    var ast = jsp.parse(orig_code); // parse code and get the initial AST
    //ast = pro.ast_mangle(ast); // get a new AST with mangled names
    ast = pro.ast_squeeze(ast); // get an AST with compression optimizations
    var finalCode = pro.gen_code(ast); // compressed code here

    console.log(finalCode + ';');
});