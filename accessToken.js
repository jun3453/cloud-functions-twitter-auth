const request = require('request');
const crypto = require('crypto');
const querystring = require('querystring');

exports.accessToken = (req, res) => {
    const requestTokenUrl = "https://api.twitter.com/oauth/access_token";
    const method = "POST";

    const apiKey = process.env.API_KEY;
    const apiSecret = process.env.API_SECRET;
    const accessTokenSecret = "";

    // set twitter authentication callback parameter
    const oauthToken = req.query["oauth_token"];
    const oauthVerifier = req.query["oauth_verifier"];

    const signatureKey = encodeURIComponent(apiSecret) + "&" + encodeURIComponent(accessTokenSecret);

    const params = {
        oauth_consumer_key: apiKey,
        oauth_signature_method: "HMAC-SHA1",
        oauth_timestamp: Math.floor((new Date()).getTime() / 1000),
        oauth_nonce: Math.floor((new Date()).getTime() / 1000),
        oauth_version: "1.0",
        oauth_token: oauthToken,
        oauth_verifier: oauthVerifier,
    }

    const requestParams = Object.keys(params).sort().map(key => {
        return encodeURIComponent(key) + "=" + encodeURIComponent(params[key])
    }).join("&");

    const encodedRequestMethod = encodeURIComponent(method);
    const encodedRequestUrl = encodeURIComponent(requestTokenUrl);
    const encodedRequestParams = encodeURIComponent(requestParams);

    const signatureData = encodedRequestMethod + "&" + encodedRequestUrl + "&" + encodedRequestParams;

    const signature = crypto.createHmac('sha1', signatureKey).update(signatureData).digest('base64');

    let encodedRequestHeaders = Object.keys(params).sort().map(key => {
        return encodeURIComponent(key) + "=" + encodeURIComponent(params[key])
    }).join(",");
    encodedRequestHeaders = encodedRequestHeaders + "," + "oauth_signature=" + encodeURIComponent(signature);

    const options = {
        uri: requestTokenUrl,
        headers: {
            Authorization: "OAuth " + encodedRequestHeaders
        }
    }

    request.post(options, (err, response, body) => {
        const result = querystring.parse(body);
        res.status(200).send(JSON.stringify(result));
    });
}
