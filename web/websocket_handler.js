'use strict';

let express = require('express');
let app = express();
let bodyParser = require('body-parser');

app.use(bodyParser.json());

app.get('/', function (req, res) {

    console.log('Get index');
    res.end('HELLOWORLD');
});

app.post('/', function (req, res) {

    let message = req.body.message;
    console.log('Regular POST message: ', message);
    return res.json({

        answer: 42
    });
});

module.exports = app;