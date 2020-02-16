exports.callbackTwitter = (req, res) => {
    // your_subdomain.page.link
    const linkName = process.env.LINK_NAME;
    // your_deep_link
    const deepLink = process.env.DEEP_LINK;
    // package_name
    const apn = process.env.APN;

    // twitter authentication callback query parameter
    const oauthToken = req.query["oauth_token"];
    const oauthVerifier = req.query["oauth_verifier"];

    const link = deepLink + "?verifier=" + oauthToken + "," + oauthVerifier;
    const redirect = "https://" + linkName + "/?link=" + link + "&apn=" + apn;
    res.redirect(redirect);
};