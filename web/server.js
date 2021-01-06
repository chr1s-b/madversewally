'use strict';

let WSServer = require('ws').Server;
let server = require('http').createServer();
let app = require('./websocket_handler.js');

// Create web socket server on top of a regular http server
let wss = new WSServer({

    server: server
});

// Also mount the app here
server.on('request', app);

// define commands (request/response codes etc)
var commands = {
    ["SUPER_SECRET_CODE_LOL"]: poo,
    ["CODE_REQ"]: process_code_req
}

function poo(conn) {
    conn.send(JSON.stringify({ answer: 43 }));
}

function process_code_req(conn) {
    conn.send("CODE_RESP.AAAA");
}

wss.on('connection', function connection(ws) {

    ws.on('message', function incoming(message) {

        console.log(`received: ${message}`);

        if (message in commands) {
            commands[message](ws);
        }

        //ws.send(JSON.stringify({answer: 43}));
    });
});

const port = process.env.PORT || 1337
server.listen(port, function () {

    console.log(`http/ws server listening on ${port}`);
});

setInterval(() => {
    console.log("ping",wss.clients.length,"clients");
    wss.clients.forEach((client) => {
        client.send(new Date().toTimeString());
    });
}, 100);