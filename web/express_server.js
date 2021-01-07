'use strict';

let express = require('express');
let app = express();

var opengames = require('./games.js');
var pages = require('./importpages.js');
console.log(pages);

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use(express.static(__dirname + '/public'));

app.get('/event_updates/', function (req, res) {
    // establish persistent sse connection
    res.writeHead(200, {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache',
        'Connection': 'keep-alive'
    });

    res.write(`data: ${pages.lobby}\n\n`);
});

app.get('/', function (req, res) {
    res.sendFile('index.html');
});

app.post('/', function (req, res) {

    var gamecode = req.body.txtCodeJoin.toUpperCase();
    var playername = req.body.txtPlayerName.toLowerCase();
    console.log(playername, "attempting to join", gamecode);
    console.log(opengames);
    if (!(gamecode in opengames)) {
        // game does not exist
        return res.end("game doesnt exist");
    }
    // game exists, add player to game
    var room = opengames[gamecode];
    // check player doesn't already exist
    if (playername in room) {
        // player already in, is it the same user?
        // TO DO
        return res.end("player already exists, choose another name");
    }
    // game exists, and a new player name, check capacity...
    if (room.numplayers >= 8) {
        // no capacity
        return res.end("sorry no capacity");
    }
    // enough capacity, so add the player to the game
    room.numplayers += 1;
    room[playername] = newplayer(res, room.numplayers == 1); // just store a score TODO add other properties
    // tell the host a player has joined
    room.conn.send("PLAYERJOIN." + playername + "&" + room.numplayers.toString());

    res.sendFile('frame.html', { root: 'public' });
    //res.end();
    return;
});

function newplayer(conn, leader) {
    // create new player object
    return {
        leader: leader,
        conn: conn,
        profile: "", // TODO
        score: 0
    };
}

module.exports = app;