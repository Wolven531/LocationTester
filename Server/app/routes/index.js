'use strict';

var express = require('express');
var router = express.Router();

router.get('/', function (req, res, next) {
    res.json({ data: {}, err: null });
});

module.exports = router;