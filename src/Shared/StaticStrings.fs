module StaticStrings

[<Literal>]
let ServiceMail = "Example@email.de"


module SameSiteUrls =
    [<Literal>]
    let PageUrl = "http://localhost:8080"

    [<Literal>]
    let logoutUrl = "/api/Account/Logout"

module OAuthPaths =

    [<Literal>]
    let GoogleOAuth = "/api/oauth/google-auth"
    [<Literal>]
    let GithubOAuth = "/api/oauth/github-auth"
    [<Literal>]
    let OrcidOAuth = "/api/oauth/orcid-auth"
    [<Literal>]
    let ExternalLoginCallback = "/api/oauth/externalLoginCallback"