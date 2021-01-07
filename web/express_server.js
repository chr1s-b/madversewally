'use strict';

let express = require('express');
let app = express();

var opengames = require('./games.js');

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use(express.static(__dirname + '/public'));

app.get('/', function (req, res) {
    res.sendFile('index.html', { root: 'public' });
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
    // check player doesn't already exist
    if (playername in opengames[gamecode]) {
        // player already in, is it the same user?
        // TO DO
        return res.end("player already exists, choose another name");
    }
    // game exists, and a new player name, so add them
    opengames[gamecode][playername] = 0; // just store a score TODO add other properties
    // tell the host a player has joined
    opengames[gamecode].conn.send("PLAYERJOIN." + playername + "&" + (Object.keys(opengames[gamecode]).length - 2).toString());
    return res.sendFile('lobby.html', { root: 'public' });
});

module.exports = app;