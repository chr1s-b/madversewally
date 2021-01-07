var fs = require('fs');

function loadfiles() {
    var x = {};
    fs.readdirSync('./pages').forEach(file => {
        var text = fs.readFileSync("./pages/"+file, "utf-8");
        x[file.split('.')[0]] = text;
    });
    return x;
}

module.exports = loadfiles();