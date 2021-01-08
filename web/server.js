'use strict';

let WSServer = require('ws').Server;
let server = require('http').createServer();
let app = require('./express_server.js');

var opengames = require('./games.js');
var show = require('./utils.js');
var pages = require('./importpages.js');

const port = process.env.PORT || 1337

// Create web socket server on top of a regular http server
let wss = new WSServer({
    server: server
});

// Also mount the app here
server.on('request', app);

// define commands (request/response codes etc)
var commands = {
    ["CODE_REQ"]: process_code_req,
    ["WRITINGSTART"]: writingstart
}

function process_code_req(conn, data) {
    let gamecode = "AAAA";
    console.log("Created new game:", gamecode);
    conn.send("CODE_RESP." + gamecode);
    // add game to opengames
    opengames[gamecode] = {
        conn: conn,
        numplayers: 0,
        players: {}
    };
}

function writingstart(conn, data) {
    console.log("writing has started, send input pages!");
    Object.keys(opengames[data].players).forEach((p) => {
        show(pages.writing1, data, p);
    });
}

wss.on('connection', function connection(ws) {

    ws.on('message', function incoming(message) {

        console.log(`received: ${message}`);
        var promptdata = message.split('.');
        if (promptdata.length == 1) {
            promptdata.push('');
        }

        if (promptdata[0] in commands) {
            console.log(promptdata[1]);
            commands[promptdata[0]](ws, promptdata[1]);
        }

    });
});

server.listen(port, function () {
    console.log(`http/ws server listening on ${port}`);
});

setInterval(() => {
    wss.clients.forEach((client) => {
        client.send(new Date().toTimeString());
    });
}, 1000);