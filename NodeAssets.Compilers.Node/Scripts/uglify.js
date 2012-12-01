var UglifyJS = require("uglify-js");

var stdin = process.openStdin();
stdin.setEncoding('utf8');

var orig_code = '';
stdin.on('data', function (chunk) {
    orig_code = orig_code + chunk;
});

stdin.on('end', function () {
    var toplevel = UglifyJS.parse(orig_code); // parse code and get the initial AST
    toplevel.figure_out_scope(); // Required before next steps
    
    // Compression
    var compressor = UglifyJS.Compressor();
    var compressed_ast = toplevel.transform(compressor);

    // Mangling
    compressed_ast.figure_out_scope();
    compressed_ast.compute_char_frequency();
    compressed_ast.mangle_names();

    // Output
    var stream = UglifyJS.OutputStream();
    compressed_ast.print(stream);
    var finalCode = stream.toString(); // this is your minified code

    console.log(finalCode);
});