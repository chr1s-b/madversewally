'use strict';

let WSServer = require('ws').Server;
let server = require('http').createServer();
let app = require('./express_server.js');

var opengames = require('./games.js');

const port = process.env.PORT || 1337

// Create web socket server on top of a regular http server
let wss = new WSServer({
    server: server
});

// Also mount the app here
server.on('request', app);

// define commands (request/response codes etc)
var commands = {
    ["CODE_REQ"]: process_code_req
}

function process_code_req(conn) {
    let gamecode = "AAAA";
    console.log("Created new game:", gamecode);
    conn.send("CODE_RESP." + gamecode);
    // add game to opengames
    opengames[gamecode] = {conn: conn, numplayers: 0};
}

wss.on('connection', function connection(ws) {

    ws.on('message', function incoming(message) {

        console.log(`received: ${message}`);

        if (message in commands) {
            commands[message](ws);
        }

    });
});

server.listen(port, function () {

    console.log(`http/ws server listening on ${port}`);
});

setInterval(() => {
    console.log("ping", JSON.stringify(wss.clients), "clients");
    wss.clients.forEach((client) => {
        client.send(new Date().toTimeString());
    });
}, 10 * 1000);