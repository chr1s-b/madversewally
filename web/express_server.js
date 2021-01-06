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
    return res.sendFile('lobby.html', { root: 'public' });
});

module.exports = app;