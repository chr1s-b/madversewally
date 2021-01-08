'use strict';

let express = require('express');
let app = express();

var opengames = require('./games.js');
var pages = require('./importpages.js');
var show = require('./utils.js');
console.log(Object.keys(pages));

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use(express.static(__dirname + '/public'));

app.get('/event_updates/:gamecode/:playername', function (req, res) {
    // establish persistent sse connection
    res.writeHead(200, {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache',
        'Connection': 'keep-alive'
    });

    if (!req.params.playername) { return res.end("invalid request"); }
    // store connection with player
    opengames[req.params.gamecode].players[req.params.playername].conn = res;

    if (opengames[req.params.gamecode].players[req.params.playername].leader) {
        show(pages.leaderlobby, req.params.gamecode, req.params.playername);
        return;
    }
    show(pages.lobby, req.params.gamecode, req.params.playername);
    return;
});

app.get('/', function (req, res) {
    res.sendFile('index.html');
});

app.post('/', function (req, res) {
    var gamecode = req.body.txtCodeJoin.toUpperCase();
    var playername = req.body.txtPlayerName.toLowerCase();
    console.log(playername, "attempting to join", gamecode);
    if (!(gamecode in opengames)) {
        // game does not exist
        return res.end("game doesnt exist");
    }
    // game exists, add player to game
    var room = opengames[gamecode];
    // check player doesn't already exist
    if (playername in room.players) {
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
    room.players[playername] = newplayer(res, room.numplayers == 1); // just store a score TODO add other properties
    console.log(opengames);
    // tell the host a player has joined
    room.conn.send("PLAYERJOIN." + playername + "&" + room.numplayers.toString());

    res.set('Content-Type', 'text/html');
    res.write(`${pages.frame.replace("{{ name }}",playername).replace("{{ gamecode }}",gamecode)}`)
    return res.end();
});

app.post('/gamestart', function (req, res) {
    var room = req.body.room;
    var player = req.body.player;
    if (opengames[room].players[player].leader) {
        // can only start if room leader (means u can't rly start another room by chance)
        // switch leader to skip tutorial page
        show(pages.leadertutorial, room, player);
        // move all players to a new screen by sse
        Object.keys(opengames[room].players).forEach((p) => {
            // don't redirect lead player
            if (p !== player) { show(pages.tutorial, room, p); }
        });
        // tell host to play tutorial
        opengames[room].conn.send("TUTORIALSTART");
        console.log("starting tutorial, closing lobby");
    }
});

app.post('/skiptutorial', function (req, res) {
    var room = req.body.room;
    var player = req.body.player;
    if (opengames[room].players[player].leader) { // check leader is doing it
        Object.keys(opengames[room].players).forEach((p) => {
            // send all players to a blank page
            show(pages.blank, room, p)
        });
        opengames[room].conn.send("TUTORIALSKIP");
        console.log("skipping tutorial");
    }
});


function newplayer(conn, leader) {
    // create new player object
    return {
        leader: leader,
        conn: null, // waits for sse connection
        profile: "", // TODO
        score: 0
    };
}

module.exports = app;