var opengames = require('./games.js');
var pages = require('./importpages.js');

function show(page, room, player) {
    opengames[room].players[player].conn.write(`data: ${Buffer.from(page.replace("{{ player }}", player).replace("{{ room }}", room)).toString('base64')}\n\n`);
    return;
}
module.exports = show;