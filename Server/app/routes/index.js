'use strict';

var express = require('express');
var router = express.Router();
var satelize = require('satelize');
var requestIp = require('request-ip');

router.get('/', function (req, res, next) {
    var returnVal = {
        err: null,
        ip1: req.clientIP || null,
        ip2: requestIp.getClientIp(req) || null,
        satData: null
    };
    var ip = returnVal.ip1 || returnVal.ip2;

    if (!ip) {
        console.error('missing ip, ip=' + JSON.stringify(ip), JSON.stringify(returnVal));

        returnVal.err = 'noIp';

        return res.json(returnVal);
    }

    satelize.satelize({ ip: ip }, function (err, satData) {
        if (err || !satData) {
            console.error(err || 'noSatData');
        }

        returnVal.err = err;
        returnVal.satData = satData;
        returnVal.usedIp = ip;

        res.json(returnVal);
    });
});

module.exports = router;